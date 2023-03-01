using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Application.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Settings;
using AdventureWorks.Domain.Models.Shield;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

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
    public void GenerateUserToken_with_null_collections_succeeds()
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

            token.Claims.Count(x => x.Type == "SecurityFunction").Should().Be(0);
            token.Claims.Count(x => x.Type == "role").Should().Be(0);
        }
    }

    [Fact]
    public void GenerateUserToken_with_full_collections_succeeds()
    {
        var model = new UserAccountModel
        {
            Id = 1,
            UserName = "john.elway",
            FirstName = "John",
            LastName = "Elway",
            PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
            MiddleName = "Albert",
            SecurityRoles = new List<SecurityRoleSlimModel>
            {
                new(){Id = 1, Name = "Help Desk Administrator"},
                new(){Id = 2, Name = "Adventure Works Employee"},
                new(){Id = 3, Name = "Adventure Works IT Employee"}
            },
            SecurityFunctions = new List<SecurityFunctionSlimModel>
            {
                new(){Id = 10, Name = "Reset User Passwords"}
            }
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

            token.Claims.Count(x => x.Type == "SecurityFunction").Should().Be(1);
            token.Claims.Count(x => x.Type == "role").Should().Be(3);
        }
    }
}
