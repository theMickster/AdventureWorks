using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Application.Services.Stores;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Profiles.Sales;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.Stores;

[ExcludeFromCodeCoverage]
public sealed class CreateStoreServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private CreateStoreService _sut;

    public CreateStoreServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new CreateStoreService(_mapper, _mockStoreRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(CreateStoreService)
                .Should().Implement<ICreateStoreService>();

            typeof(CreateStoreService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new CreateStoreService(
                    null!,
                    _mockStoreRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new CreateStoreService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");
        }
    }
}
