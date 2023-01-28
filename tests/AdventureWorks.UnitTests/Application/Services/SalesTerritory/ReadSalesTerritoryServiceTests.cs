using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.SalesTerritory;
using AdventureWorks.Application.Services.SalesTerritory;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Profiles;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.SalesTerritory;

[ExcludeFromCodeCoverage]
public sealed class ReadSalesTerritoryServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesTerritoryRepository> _mockRepository = new();
    private ReadSalesTerritoryService _sut;

    public ReadSalesTerritoryServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesTerritoryEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadSalesTerritoryService(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadSalesTerritoryService)
                .Should().Implement<IReadSalesTerritoryService>();

            typeof(ReadSalesTerritoryService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesTerritoryService(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSalesTerritoryService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("salesTerritoryRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SalesTerritoryEntity)null!);

        var result = await _sut.GetByIdAsync(12).ConfigureAwait(false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new SalesTerritoryEntity { TerritoryId = 1, Name = "Central" });

        var result = await _sut.GetByIdAsync(1).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Central");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<SalesTerritoryEntity>)null!);

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<SalesTerritoryEntity>());

        result = await _sut.GetListAsync().ConfigureAwait(false);
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

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        result.Count.Should().Be(4);
    }
}
