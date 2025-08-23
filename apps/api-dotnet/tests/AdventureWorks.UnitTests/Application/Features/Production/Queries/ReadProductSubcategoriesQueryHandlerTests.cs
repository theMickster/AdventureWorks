using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductSubcategoriesQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly ReadProductSubcategoriesQueryHandler _sut;

    public ReadProductSubcategoriesQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductSubcategoryToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadProductSubcategoriesQueryHandler(_mapper, _mockProductRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_returns_subcategories_for_categoryAsync()
    {
        var subcategories = ProductionDomainFixtures.GetMockProductSubcategories();

        _mockProductRepository.Setup(x => x.GetProductSubcategoriesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subcategories.AsReadOnly());

        var result = await _sut.Handle(new ReadProductSubcategoriesQuery { CategoryId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            result[0].Name.Should().Be("Mountain Bikes");
            result[0].CategoryName.Should().Be("Bikes");
        }
    }

    [Fact]
    public async Task Handle_returns_all_subcategories_when_no_filterAsync()
    {
        var subcategories = ProductionDomainFixtures.GetMockProductSubcategories();

        _mockProductRepository.Setup(x => x.GetProductSubcategoriesAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subcategories.AsReadOnly());

        var result = await _sut.Handle(new ReadProductSubcategoriesQuery { CategoryId = null }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Count.Should().Be(3);
    }
}
