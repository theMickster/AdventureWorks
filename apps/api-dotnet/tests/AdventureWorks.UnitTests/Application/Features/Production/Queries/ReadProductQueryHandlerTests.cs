using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly ReadProductQueryHandler _sut;

    public ReadProductQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductToDetailModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadProductQueryHandler(_mapper, _mockProductRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_returns_null_when_not_foundAsync()
    {
        _mockProductRepository.Setup(x => x.GetProductByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _sut.Handle(new ReadProductQuery { Id = 999 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_detail_model_when_foundAsync()
    {
        var product = ProductionDomainFixtures.GetMockProducts().First();

        _mockProductRepository.Setup(x => x.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _sut.Handle(new ReadProductQuery { Id = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Mountain Bike");
            result.ProductNumber.Should().Be("BK-M82S-38");
            result.TotalInventoryQuantity.Should().Be(100);
        }
    }
}
