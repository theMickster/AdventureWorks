using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesTerritoryQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesTerritoryRepository> _mockRepository = new();
    private ReadSalesTerritoryQueryHandler _sut;

    public ReadSalesTerritoryQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesTerritoryEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadSalesTerritoryQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesTerritoryQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSalesTerritoryQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("salesTerritoryRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SalesTerritoryEntity)null!);

        var result = await _sut.Handle( new ReadSalesTerritoryQuery{Id = 12}, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new SalesTerritoryEntity { TerritoryId = 1, Name = "Central" });

        var result = await _sut.Handle(new ReadSalesTerritoryQuery{ Id = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Central");
        }
    }
}
