using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductCategoriesQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly ReadProductCategoriesQueryHandler _sut;

    public ReadProductCategoriesQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductCategoryToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadProductCategoriesQueryHandler(_mapper, _mockProductRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public async Task Handle_returns_categoriesAsync()
    {
        var categories = ProductionDomainFixtures.GetMockProductCategories();

        _mockProductRepository.Setup(x => x.GetProductCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories.AsReadOnly());

        var result = await _sut.Handle(new ReadProductCategoriesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(4);
            result[0].Name.Should().Be("Bikes");
        }
    }
}
