using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Application.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.AccountInfo;
using AdventureWorks.Domain.Models.AccountInfo;
using AdventureWorks.Domain.Profiles;
using AdventureWorks.Test.Common.Extensions;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Services.Login;

[ExcludeFromCodeCoverage]
public sealed class ReadUserLoginServiceTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadUserLoginService>> _mockLogger = new();
    private readonly Mock<IUserAccountRepository> _mockUserAccountRepository = new();
    private readonly Mock<ITokenService> _mockTokenService = new();
    private readonly IMapper _mapper;
    private ReadUserLoginService _sut;

    public ReadUserLoginServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(UserAccountEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadUserLoginService(_mockLogger.Object, _mockUserAccountRepository.Object, _mockTokenService.Object, _mapper);
    }
    
    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadUserLoginService)
                .Should().Implement<IReadUserLoginService>();

            typeof(ReadUserLoginService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadUserLoginService(
                    null!,
                    _mockUserAccountRepository.Object, 
                    _mockTokenService.Object, 
                    _mapper)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _sut = new ReadUserLoginService(
                    _mockLogger.Object,
                    null!, 
                    _mockTokenService.Object, 
                    _mapper)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("userAccountRepository");

            _ = ((Action)(() => _sut = new ReadUserLoginService(
                    _mockLogger.Object,
                    _mockUserAccountRepository.Object,
                    null!,
                    _mapper)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("tokenService");

            _ = ((Action)(() => _sut = new ReadUserLoginService(
                    _mockLogger.Object,
                    _mockUserAccountRepository.Object,
                    _mockTokenService.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

        }
    }

    [Fact]
    public async Task AuthenticateUserAsync_returns_correctly_when_username_mismatchedAsync()
    {
        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync("rod.smith"))
            .ReturnsAsync((UserAccountEntity)null!);

        var (user, token, validationFailures) = await _sut.AuthenticateUserAsync("rod.smith", "aPassword").ConfigureAwait(false);

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-001").Should().Be(1);
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-002").Should().Be(0);

            _mockLogger.VerifyLoggingMessageIs("Auth-Error-001 - Username does not exist", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task AuthenticateUserAsync_returns_correctly_when_password_is_invalidAsync()
    {
        const string password = "$2a$11$t5Qm3EQLYUYR089uS5NPYeBRB.Vm008dMH4DZOT5CfvBZh3RvlZkS";
        const string username = "john.elway";

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK"
            });

        var (user, token, validationFailures) = await _sut.AuthenticateUserAsync(username, password).ConfigureAwait(false);

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-001").Should().Be(0);
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-002").Should().Be(1);

            _mockLogger.VerifyLoggingMessageContains("Auth-Error-002", null, LogLevel.Information);
            _mockLogger.VerifyLoggingMessageContains("Password does not match", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task AuthenticateUserAsync_succeeds_when_all_is_goodAsync()
    {
        const string passwordHash = "$2a$11$WzjLdJ.9Mg4Gk96gcSsYeu7tUqRtX5P02OV1Pe5A//UWRF52WoWYe";
        const string password = "HelloWorld";
        const string username = "john.elway";
        var tokenModel = new UserAccountTokenModel()
        {
            Id = new Guid(),
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBZHZlbnR1cmVXb3Jrc0FQSSIsImp0aSI6IjJlNTBiODM3LTYyNjAtNDljMy1hNTMxLTgzOTUzY2U1NzcyMiIsImlhdCI6MTY3NjA1NjcyNCwiZXhwIjoxNjc2MDYyNzI0LCJnaXZlbl9uYW1lIjoiSm9obiIsImZhbWlseV9uYW1lIjoiRWx3YXkiLCJVc2VySWQiOiIxIiwiVXNlck5hbWUiOiJqb2huLmVsd2F5IiwibmJmIjoxNjc2MDU2NzI0LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdC8iLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdC8ifQ.gl90yhJtcPtfTrYtgIX7nWCKpaOMUyU2Ajbc7B8FNKQ",
            TokenExpiration = DateTime.UtcNow.AddSeconds(120),
            RefreshToken = string.Empty,
            RefreshTokenExpiration = DateTime.MinValue
        };

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = passwordHash
            });

        _mockTokenService.Setup(x => x.GenerateUserToken(It.IsAny<UserAccountModel>()))
            .Returns(tokenModel);

        var (user, outputToken, validationFailures) = await _sut.AuthenticateUserAsync(username, password).ConfigureAwait(false);

        using (new AssertionScope())
        {
            user.Should().NotBeNull();
            user!.UserName.Should().Be(username);
            outputToken.Should().NotBeNull();
            validationFailures.Count.Should().Be(0);
        }
    }
}
