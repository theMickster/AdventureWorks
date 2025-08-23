using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductInventoryQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly ReadProductInventoryQueryHandler _sut;

    public ReadProductInventoryQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductInventoryToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadProductInventoryQueryHandler(_mapper, _mockProductRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_returns_inventory_for_productAsync()
    {
        var inventory = ProductionDomainFixtures.GetMockProductInventory(1);

        _mockProductRepository.Setup(x => x.GetProductInventoryByProductIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory.AsReadOnly());

        var result = await _sut.Handle(new ReadProductInventoryQuery { ProductId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            result[0].LocationName.Should().Be("Tool Crib");
            result[0].Quantity.Should().Be(50);
        }
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_no_inventoryAsync()
    {
        _mockProductRepository.Setup(x => x.GetProductInventoryByProductIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductInventory>().AsReadOnly());

        var result = await _sut.Handle(new ReadProductInventoryQuery { ProductId = 999 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Count.Should().Be(0);
    }
}
