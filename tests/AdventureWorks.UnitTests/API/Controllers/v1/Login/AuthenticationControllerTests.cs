using AdventureWorks.API.Controllers.v1.Login;
using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Test.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using AdventureWorks.Common.Settings;
using FluentValidation.Results;
using AdventureWorks.Domain.Models;
using AdventureWorks.Domain.Models.Shield;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Login;

[ExcludeFromCodeCoverage]
public sealed class AuthenticationControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<AuthenticationController>> _mockLogger = new();
    private readonly Mock<IReadUserLoginService> _mockUserLoginService = new();
    private readonly Mock<IOptionsSnapshot<TokenSettings>> _mockOptionsSnapshotConfig = new();
    private AuthenticationController _sut;

    public AuthenticationControllerTests()
    {
        _mockOptionsSnapshotConfig.Setup(i => i.Value)
            .Returns(new TokenSettings
            {
                Subject = "AdventureWorksAPI",
                Key = "HelloWorldThisIsASubParSecretKey",
                Issuer = "https://localhost/",
                Audience = "https://localhost/",
                TokenExpirationInSeconds = 6000,
                RefreshTokenExpirationInDays = 2
            });

        _sut = new AuthenticationController(_mockLogger.Object, _mockUserLoginService.Object, _mockOptionsSnapshotConfig.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Connection = { Id = "abc", RemoteIpAddress = new IPAddress(16885952) }
                }
            }
        };
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new AuthenticationController(null!, _mockUserLoginService.Object, _mockOptionsSnapshotConfig.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _sut = new AuthenticationController(_mockLogger.Object, null!, _mockOptionsSnapshotConfig.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("userLoginService");

            _ = ((Action)(() => _sut = new AuthenticationController(_mockLogger.Object, _mockUserLoginService.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("tokenSettings");
        }
    }

    [Fact]
    public async Task AuthenticateUser_fails_when_model_is_nullAsync()
    {
        const string message = "Invalid authentication request.";
        var result = await _sut.AuthenticateUser(null).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be(message);

            _mockLogger.VerifyLoggingMessageContains(message, null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task AuthenticateUser_fails_when_username_is_nullAsync()
    {
        const string message = "Invalid authentication request; Username and Password are required.";
        var model = new AuthenticationRequestModel { Username = null, Password = "hello" };
        var result = await _sut.AuthenticateUser(model).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be(message);

            model.Password.Should().Be("hello");
            model.Username?.Should().BeNull();

            _mockLogger.VerifyLoggingMessageContains(message, null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task AuthenticateUser_fails_when_username_is_emptyAsync()
    {
        const string message = "Invalid authentication request; Username and Password are required.";
        var model = new AuthenticationRequestModel { Username = "   ", Password = "hello" };
        var result = await _sut.AuthenticateUser(model).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be(message);

            model.Password.Should().Be("hello");
            model.Username.Trim().Should().BeEmpty();

            _mockLogger.VerifyLoggingMessageContains(message, null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task AuthenticateUser_fails_when_password_is_nullAsync()
    {
        const string message = "Invalid authentication request; Username and Password are required.";
        var model = new AuthenticationRequestModel { Username = "hello.world", Password = null };
        var result = await _sut.AuthenticateUser(model).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be(message);

            model.Username.Should().Be("hello.world");
            model.Password?.Should().BeNull();

            _mockLogger.VerifyLoggingMessageContains(message, null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task AuthenticateUser_fails_when_password_is_emptyAsync()
    {
        const string message = "Invalid authentication request; Username and Password are required.";
        var model = new AuthenticationRequestModel { Username = "hello.world", Password = "" };
        var result = await _sut.AuthenticateUser(model).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be(message);

            model.Username.Should().Be("hello.world");
            model.Password.Should().BeEmpty();

            _mockLogger.VerifyLoggingMessageContains(message, null, LogLevel.Information);
        }
    }

    [Theory]
    [MemberData(nameof(AuthenticateUserData))]
    public async Task AuthenticateUser_fails_when_authenticationService_fails_01Async(
        UserAccountModel? user, 
        UserAccountTokenModel? tokenModel, 
        List<ValidationFailure> errors,
        string expectedLoggedMessage
        )
    {
        _mockUserLoginService.Setup(x => x.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((user, tokenModel, errors));

        const string message = "Unable to complete authentication request.";
        var model = new AuthenticationRequestModel { Username = "hello.world", Password = "hello" };
        var result = await _sut.AuthenticateUser(model).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be(message);

            _mockLogger.VerifyLoggingMessageContains(expectedLoggedMessage, null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task AuthenticateUser_succeedsAsync()
    {
        var user = new UserAccountModel
        {
            Id = 1,
            FirstName = "Joe",
            LastName = "Montanta",
            UserName = "joe.montanta",
            PrimaryEmailAddress = "joe.montanta@example.com",
            SecurityRoles = new List<SecurityRoleSlimModel>
            {
                new(){Id = 1, Name = "Help Desk Administrator"},
                new(){Id = 2, Name = "Adventure Works Employee"},
                new(){Id = 3, Name = "Adventure Works IT Employee"}
            },
            SecurityFunctions = new List<SecurityFunctionSlimModel>
            {
                new(){Id = 10, Name = "Reset User Passwords"}
            },
            SecurityGroups = new List<SecurityGroupSlimModel>
            {
                new(){Id = 1, Name = "A Group 001"},
                new(){Id = 2, Name = "A Group 002"}
            }
        };

        var tokenModel = new UserAccountTokenModel
        {
            Id = new Guid(),
            Token = "token",
            TokenExpiration = DateTime.UtcNow.AddSeconds(120),
            RefreshToken = "refreshToken",
            RefreshTokenExpiration = DateTime.UtcNow.AddSeconds(180)
        };

        _mockUserLoginService.Setup(x => x.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((user, tokenModel, new List<ValidationFailure>()));
        
        var model = new AuthenticationRequestModel { Username = "hello.world", Password = "hello" };
        var result = await _sut.AuthenticateUser(model).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        var outputModel = objectResult!.Value! as AuthenticationResponseModel;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            outputModel.Should().NotBeNull();
            outputModel!.Token.Should().NotBeNull();
            outputModel!.Username.Should().Be("joe.montanta");
            outputModel!.FullName.Should().Be("Montanta, Joe");
            outputModel!.EmailAddress.Should().Be("joe.montanta@example.com");
            outputModel!.Token.Token.Should().Be("token");
            outputModel!.Token.RefreshToken.Should().Be("refreshToken");
            outputModel!.Token.TokenExpiration.Should().BeAfter(DateTime.Now);
            outputModel!.Token.RefreshTokenExpiration.Should().BeAfter(DateTime.Now);

            outputModel!.SecurityFunctions.Count.Should().Be(1);
            outputModel!.SecurityGroups.Count.Should().Be(2);
            outputModel!.SecurityRoles.Count.Should().Be(3);
        }
    }


    #region Xunit Theory Test Data

    public static IEnumerable<object?[]> AuthenticateUserData =>
        new List<object?[]>
        {
            new object?[] { new UserAccountModel(), new UserAccountTokenModel(), new List<ValidationFailure> {new (){ErrorCode = "1", ErrorMessage = "A"}}, "User Authentication Attempt Failed" }
            ,new object?[] { null, new UserAccountTokenModel(), new List<ValidationFailure>(), "Unable to complete authentication request" }
            ,new object?[] { new UserAccountModel(), null, new List<ValidationFailure>(), "Unable to complete authentication request" }
        };

    #endregion Xunit Theory Test Data
}
