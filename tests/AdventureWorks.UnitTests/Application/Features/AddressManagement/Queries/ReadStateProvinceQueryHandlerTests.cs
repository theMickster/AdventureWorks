using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.AddressManagement.Queries;

public sealed class ReadStateProvinceQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStateProvinceRepository> _mockStateProvinceRepository = new();
    private ReadStateProvinceQueryHandler _sut;

    public ReadStateProvinceQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StateProvinceEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStateProvinceQueryHandler(_mapper, _mockStateProvinceRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStateProvinceQueryHandler(
                    null!,
                    _mockStateProvinceRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStateProvinceQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("repository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockStateProvinceRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((StateProvinceEntity)null!);

        var result = await _sut.Handle(new ReadStateProvinceQuery{Id = 12}, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockStateProvinceRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new StateProvinceEntity
                { StateProvinceId = 1, Name = "A State", CountryRegionCode = "UK", TerritoryId = 7, IsOnlyStateProvinceFlag = false });

        var result = await _sut.Handle(new ReadStateProvinceQuery { Id = 15 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Name.Should().Be("A State");
        }
    }

}
