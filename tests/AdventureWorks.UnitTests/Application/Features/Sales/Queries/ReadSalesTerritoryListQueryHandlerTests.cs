using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesTerritoryListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesTerritoryRepository> _mockRepository = new();
    private ReadSalesTerritoryListQueryHandler _sut;

    public ReadSalesTerritoryListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesTerritoryEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadSalesTerritoryListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesTerritoryListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSalesTerritoryListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("salesTerritoryRepository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<SalesTerritoryEntity>)null!);

        var result = await _sut.Handle( new ReadSalesTerritoryListQuery(), CancellationToken.None );
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<SalesTerritoryEntity>());

        result = await _sut.Handle(new ReadSalesTerritoryListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<SalesTerritoryEntity>
            {
                new() {TerritoryId = 1, Name = "Central"}
                ,new() {TerritoryId = 2, Name = "East"}
                ,new() {TerritoryId = 3, Name = "West"}
                ,new() {TerritoryId = 4, Name = "Northwest"}
            });

        var result = await _sut.Handle(new ReadSalesTerritoryListQuery(), CancellationToken.None);
        result.Count.Should().Be(4);
    }
}
