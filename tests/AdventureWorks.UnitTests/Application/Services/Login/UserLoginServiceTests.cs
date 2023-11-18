using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Repositories.AccountInfo;
using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Application.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;
using AdventureWorks.Domain.Profiles;
using AdventureWorks.Test.Common.Extensions;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Services.Login;

[ExcludeFromCodeCoverage]
public sealed class UserLoginServiceTests : UnitTestBase
{
    private readonly Mock<ILogger<UserLoginService>> _mockLogger = new();
    private readonly Mock<IUserAccountRepository> _mockUserAccountRepository = new();
    private readonly Mock<IReadUserAuthorizationRepository> _mockReadUserAuthorizationRepository = new();
    private readonly Mock<ITokenService> _mockTokenService = new();
    private readonly Mock<IUserRefreshTokenRepository> _mockUserRefreshTokenRepository = new();
    private readonly IMapper _mapper;
    private UserLoginService _sut;

    public UserLoginServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(UserAccountEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new UserLoginService(
            _mockLogger.Object, 
            _mockUserAccountRepository.Object, 
            _mockReadUserAuthorizationRepository.Object, 
            _mockTokenService.Object, 
            _mapper,
            _mockUserRefreshTokenRepository.Object);
    }
    
    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(UserLoginService)
                .Should().Implement<IUserLoginService>();

            typeof(UserLoginService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new UserLoginService(
                    null!,
                    _mockUserAccountRepository.Object,
                    _mockReadUserAuthorizationRepository.Object,
                    _mockTokenService.Object, 
                    _mapper,
                    _mockUserRefreshTokenRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _sut = new UserLoginService(
                    _mockLogger.Object,
                    null!,
                    _mockReadUserAuthorizationRepository.Object,
                    _mockTokenService.Object, 
                    _mapper,
                    _mockUserRefreshTokenRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("userAccountRepository");


            _ = ((Action)(() => _sut = new UserLoginService(
                    _mockLogger.Object,
                    _mockUserAccountRepository.Object,
                    null!,
                    _mockTokenService.Object,
                    _mapper,
                    _mockUserRefreshTokenRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readUserAuthorizationRepository");


            _ = ((Action)(() => _sut = new UserLoginService(
                    _mockLogger.Object,
                    _mockUserAccountRepository.Object,
                    _mockReadUserAuthorizationRepository.Object,
                    null!,
                    _mapper,
                    _mockUserRefreshTokenRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("tokenService");

            _ = ((Action)(() => _sut = new UserLoginService(
                    _mockLogger.Object,
                    _mockUserAccountRepository.Object,
                    _mockReadUserAuthorizationRepository.Object,
                    _mockTokenService.Object,
                    null!,
                    _mockUserRefreshTokenRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");
            
            _ = ((Action)(() => _sut = new UserLoginService(
                    _mockLogger.Object,
                    _mockUserAccountRepository.Object,
                    _mockReadUserAuthorizationRepository.Object,
                    _mockTokenService.Object,
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("userRefreshTokenRepository");
        }
    }

    [Fact]
    public async Task AuthenticateUserAsync_returns_correctly_when_username_mismatchedAsync()
    {
        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync("rod.smith"))
            .ReturnsAsync((UserAccountEntity)null!);

        var (user, token, validationFailures) = await _sut.AuthenticateUserAsync("rod.smith", "aPassword", "192.168.100.69");

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

        var (user, token, validationFailures) = await _sut.AuthenticateUserAsync(username, password, "192.168.100.69");

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
    public async Task AuthenticateUserAsync_returns_correctly_when_user_authorization_entity_is_not_foundAsync()
    {
        const string passwordHash = "$2a$11$WzjLdJ.9Mg4Gk96gcSsYeu7tUqRtX5P02OV1Pe5A//UWRF52WoWYe";
        const string password = "HelloWorld";
        const string username = "john.elway";

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = passwordHash
            });

        _mockReadUserAuthorizationRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync( (UserAuthorizationEntity)null!);

        var (user, token, validationFailures) = await _sut.AuthenticateUserAsync(username, password, "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-001").Should().Be(0);
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-002").Should().Be(0);
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-003").Should().Be(1);

            _mockLogger.VerifyLoggingMessageContains("Auth-Error-003", null, LogLevel.Information);
            _mockLogger.VerifyLoggingMessageContains("Unable to construct authorization entity as user permission mappings do not exist", null, LogLevel.Information);
        }
    }
    
    [Fact]
    public async Task AuthenticateUserAsync_succeeds_when_all_is_goodAsync()
    {
        var (username, password, refreshToken) = SetupAuthHappyPath();

        var (user, outputToken, validationFailures) = await _sut.AuthenticateUserAsync(username, password, "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().NotBeNull();
            user!.UserName.Should().Be(username);
            outputToken.Should().NotBeNull();
            validationFailures.Count.Should().Be(0);

            user!.SecurityFunctions.Count.Should().Be(3);
            user!.SecurityRoles.Count.Should().Be(2);
            user!.SecurityGroups.Count.Should().Be(4);
            
            user!.SecurityFunctions.Count(x => x.Id == 3).Should().Be(1);
            user!.SecurityRoles.Count(x => x.Id == 11).Should().Be(1);
            user!.SecurityGroups.Count(x => x.Id == 104).Should().Be(1);
        }
    }

    [Fact]
    public async Task RefreshTokenAsync_returns_correctly_when_username_mismatchedAsync()
    {
        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync("rod.smith"))
            .ReturnsAsync((UserAccountEntity)null!);

        var (user, token, validationFailures) = await _sut.RefreshTokenAsync("aRefreshTokenGoesHere", "rod.smith", "192.168.100.69");

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
    public async Task RefreshTokenAsync_returns_correctly_when_refresh_token_not_foundAsync()
    {
        const string username = "john.elway";

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = "$2a$11$t5"
            });

        _mockUserRefreshTokenRepository
            .Setup(x => x.GetRefreshTokenListByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UserRefreshTokenEntity>());

        var (user, token, validationFailures) = await _sut.RefreshTokenAsync("aRefreshTokenGoesHere", "john.elway", "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-004").Should().Be(1);

            _mockLogger.VerifyLoggingMessageIs("Auth-Error-004 - Unable to create new auth token! The refresh token does not exist for the given user.", null, LogLevel.Information);
        }
    }

    [Fact]
    [SuppressMessage("Async", "AsyncifyInvocation:Use Task Async", Justification = "...")]
    public async Task RefreshTokenAsync_returns_correctly_when_multiple_refresh_tokens_foundAsync()
    {
        const string username = "john.elway";

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = "$2a$11$t5"
            });

        _mockUserRefreshTokenRepository
            .Setup(x => x.GetRefreshTokenListByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UserRefreshTokenEntity> {new() {BusinessEntityId = 1}, new (){BusinessEntityId = 1}});

        _mockUserRefreshTokenRepository.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<UserRefreshTokenEntity>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var (user, token, validationFailures) = await _sut.RefreshTokenAsync("aRefreshTokenGoesHere", "john.elway", "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-005").Should().Be(1);

            _mockLogger.VerifyLoggingMessageIs("Auth-Error-005 - Unable to create new auth token! The system does not support duplicate refresh token entries for the same user.", null, LogLevel.Information);

            _mockUserRefreshTokenRepository.Verify( x => x.RevokeRefreshTokenAsync(It.IsAny<UserRefreshTokenEntity>()), Times.Exactly(2));
        }
    }
    
    [Fact]
    public async Task RefreshTokenAsync_returns_correctly_when_refresh_token_is_expiredAsync()
    {
        const string username = "john.elway";

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = "$2a$11$t5"
            });

        _mockUserRefreshTokenRepository
            .Setup(x => x.GetRefreshTokenListByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UserRefreshTokenEntity> { new() { BusinessEntityId = 1,IsExpired = true} });

        var (user, token, validationFailures) = await _sut.RefreshTokenAsync("aRefreshTokenGoesHere", "john.elway", "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-006").Should().Be(1);

            _mockLogger.VerifyLoggingMessageIs("Auth-Error-006 - Unable to create new auth token! The supplied refresh token is expired!", null, LogLevel.Information);
        }
    }
    
    [Fact]
    public async Task RefreshTokenAsync_returns_correctly_when_refresh_token_is_revokedAsync()
    {
        const string username = "john.elway";

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = "$2a$11$t5"
            });

        _mockUserRefreshTokenRepository
            .Setup(x => x.GetRefreshTokenListByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UserRefreshTokenEntity> { new() { BusinessEntityId = 1, IsExpired = false, IsRevoked = true} });

        var (user, token, validationFailures) = await _sut.RefreshTokenAsync("aRefreshTokenGoesHere", "john.elway", "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-007").Should().Be(1);

            _mockLogger.VerifyLoggingMessageIs("Auth-Error-007 - Unable to create new auth token! The supplied refresh token is revoked!", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task RefreshTokenAsync_returns_correctly_when_user_authorization_entity_is_not_foundAsync()
    {
        const string username = "john.elway";

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(new UserAccountEntity
            {
                BusinessEntityId = 1,
                UserName = username,
                PasswordHash = "$2a$11$t5"
            });

        _mockUserRefreshTokenRepository
            .Setup(x => x.GetRefreshTokenListByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UserRefreshTokenEntity> { new() { BusinessEntityId = 1, IsExpired = false, IsRevoked = false} });

        _mockReadUserAuthorizationRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync((UserAuthorizationEntity)null!);

        var (user, token, validationFailures) = await _sut.RefreshTokenAsync("aRefreshTokenGoesHere", "john.elway", "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().BeNull();
            token.Should().BeNull();
            validationFailures.Count(x => x.ErrorCode == "Auth-Error-003").Should().Be(1);

            _mockLogger.VerifyLoggingMessageContains("Auth-Error-003", null, LogLevel.Information);
            _mockLogger.VerifyLoggingMessageContains("Unable to construct authorization entity as user permission mappings do not exist", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task RefreshTokenAsync_succeeds_when_all_is_goodAsync()
    {
        var (username, password, refreshToken) = SetupAuthHappyPath();

        var (user, outputToken, validationFailures) = await _sut.RefreshTokenAsync(refreshToken, username, "192.168.100.69");

        using (new AssertionScope())
        {
            user.Should().NotBeNull();
            user!.UserName.Should().Be(username);
            outputToken.Should().NotBeNull();
            validationFailures.Count.Should().Be(0);

            user!.SecurityFunctions.Count.Should().Be(3);
            user!.SecurityRoles.Count.Should().Be(2);
            user!.SecurityGroups.Count.Should().Be(4);

            user!.SecurityFunctions.Count(x => x.Id == 3).Should().Be(1);
            user!.SecurityRoles.Count(x => x.Id == 11).Should().Be(1);
            user!.SecurityGroups.Count(x => x.Id == 104).Should().Be(1);
        }
    }

    private (string, string, string) SetupAuthHappyPath()
    {
        const string passwordHash = "$2a$11$WzjLdJ.9Mg4Gk96gcSsYeu7tUqRtX5P02OV1Pe5A//UWRF52WoWYe";
        const string password = "HelloWorld";
        const string username = "john.elway";
        const string refreshToken = "3d664dabe0d14d7291053530dc6379f0ac37a8ebc2ad467abc988a23a09ee717";

        var tokenModel = new UserAccountTokenModel
        {
            Id = new Guid(),
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBZHZlbnR1cmVXb3Jrc0FQSSIsImp0aSI6IjJlNTBiODM3LTYyNjAtNDljMy1hNTMxLTgzOTUzY2U1NzcyMiIsImlhdCI6MTY3NjA1NjcyNCwiZXhwIjoxNjc2MDYyNzI0LCJnaXZlbl9uYW1lIjoiSm9obiIsImZhbWlseV9uYW1lIjoiRWx3YXkiLCJVc2VySWQiOiIxIiwiVXNlck5hbWUiOiJqb2huLmVsd2F5IiwibmJmIjoxNjc2MDU2NzI0LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdC8iLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdC8ifQ.gl90yhJtcPtfTrYtgIX7nWCKpaOMUyU2Ajbc7B8FNKQ",
            TokenExpiration = DateTime.UtcNow.AddSeconds(120),
            RefreshToken = refreshToken,
            RefreshTokenExpiration = DateTime.UtcNow.AddHours(24)
        };

        var userAccountEntity = new UserAccountEntity
        {
            BusinessEntityId = 1,
            UserName = username,
            PasswordHash = passwordHash
        };

        _mockUserAccountRepository.Setup(x => x.GetByUserNameAsync(username))
            .ReturnsAsync(userAccountEntity);

        _mockTokenService.Setup(x => x.GenerateUserTokenAsync(It.IsAny<UserAccountModel>(), It.IsAny<string>()))
            .ReturnsAsync(tokenModel);

        _mockReadUserAuthorizationRepository.Setup(x => x.GetByUserIdAsync(1))
            .ReturnsAsync(new UserAuthorizationEntity
            {
                BusinessEntityId = 1,
                SecurityFunctions = new List<SecurityFunctionEntity>
                {
                    new() {Id = 1, Name = "Super Cool Function"},
                    new() {Id = 2, Name = "Kinda Cool Function"},
                    new() {Id = 3, Name = "Not Cool Function"}
                }.AsReadOnly(),
                SecurityRoles = new List<SecurityRoleEntity>
                {
                    new() {Id = 10, Name = "Regular Employee"},
                    new() {Id = 11, Name = "IT Teammate"}
                }.AsReadOnly(),
                SecurityGroups = new List<SecurityGroupEntity>
                {
                    new() {Id = 101, Name = "Adventure Works Employees"},
                    new() {Id = 102, Name = "Red Team Go"},
                    new() {Id = 103, Name = "Weekend Tier 3 Team"},
                    new() {Id = 104, Name = "Denver IT 04 Team"}
                }.AsReadOnly()
            });

        _mockUserRefreshTokenRepository
            .Setup(x => x.GetRefreshTokenListByUserIdAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new List<UserRefreshTokenEntity>
            {
                new()
                {
                    RecordId = new Guid("fba3d756-8613-423c-bb01-e7b03d196852"),
                    UserRefreshTokenId = 1,
                    BusinessEntityId =  1,
                    IsExpired = false,
                    IpAddress = "192.168.100.69",
                    ExpiresOn = DateTime.UtcNow.AddHours(24),
                    RefreshToken = refreshToken,
                    UserAccountEntity = userAccountEntity,
                    IsRevoked = false
                }
            });

        return (username, password, refreshToken);
    }

}
