using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Application.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Settings;
using Microsoft.Extensions.Options;

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
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new TokenService(null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("tokenSettings");
            
        }
    }
}
