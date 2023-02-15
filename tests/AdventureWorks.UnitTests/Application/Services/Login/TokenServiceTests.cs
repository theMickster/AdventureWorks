using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Application.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Settings;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using AdventureWorks.Domain.Models.Shield;

namespace AdventureWorks.UnitTests.Application.Services.Login;

[ExcludeFromCodeCoverage]
public sealed class TokenServiceTests : UnitTestBase
{
    private readonly Mock<IOptionsSnapshot<TokenSettings>> _mockOptionsSnapshotConfig = new();
    private TokenService _sut;

    public TokenServiceTests()
    {
        _mockOptionsSnapshotConfig.Setup(i => i.Value)
            .Returns(new TokenSettings
            {
                Subject = "AdventureWorksAPI",
                Key = "HelloWorldThisIsASubParSecretKey",
                Issuer = "https://localhost/",
                Audience= "https://localhost/",
                TokenExpirationInSeconds = 6000
            });

        _sut = new TokenService(_mockOptionsSnapshotConfig.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(TokenService)
                .Should().Implement<ITokenService>();

            typeof(TokenService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new TokenService(null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("tokenSettings");
        }
    }

    [Fact]
    public void GenerateUserToken_succeeds()
    {
        var model = new UserAccountModel
        {
            Id = 1,
            UserName = "john.elway",
            FirstName = "John",
            LastName = "Elway",
            PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
            MiddleName = "Albert"
        };

        var result = _sut.GenerateUserToken(model);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.TokenExpiration.Should().BeAfter(DateTime.UtcNow);
            result.Id.Should().NotBeEmpty();
            result.RefreshToken.Should().BeNullOrWhiteSpace();
            result.RefreshTokenExpiration.Should().Be(DateTime.MinValue);

            token.Claims.FirstOrDefault(x => x.Type == "sub")!.Value.Should().Be("AdventureWorksAPI");
            token.Claims.FirstOrDefault(x => x.Type == "iss")!.Value.Should().Be("https://localhost/");
            token.Claims.FirstOrDefault(x => x.Type == "aud")!.Value.Should().Be("https://localhost/");
            token.Claims.FirstOrDefault(x => x.Type == "given_name")!.Value.Should().Be(model.FirstName);
            token.Claims.FirstOrDefault(x => x.Type == "family_name")!.Value.Should().Be(model.LastName);
            token.Claims.FirstOrDefault(x => x.Type == "UserName")!.Value.Should().Be(model.UserName);
        }
    }
}
