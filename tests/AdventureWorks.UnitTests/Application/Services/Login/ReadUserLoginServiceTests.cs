using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.Login;
using AdventureWorks.Application.Services.Login;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Profiles;
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
}
