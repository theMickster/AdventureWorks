using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Models.Features.Production;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class ProductModelToLookupModelsProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public ProductModelToLookupModelsProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ProductModelToLookupModelsProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Maps_to_list_model_correctly()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new AdventureWorks.Domain.Entities.Production.ProductModel
        {
            ProductModelId = 1,
            Name = "Classic Vest",
            CatalogDescription = "<catalog>vest</catalog>",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<ProductModelListModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ProductModelId.Should().Be(1);
            result.Name.Should().Be("Classic Vest");
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }

    [Fact]
    public void Maps_to_detail_model_correctly()
    {
        var modifiedDate = new DateTime(2026, 5, 4, 0, 0, 0, DateTimeKind.Utc);
        var entity = new AdventureWorks.Domain.Entities.Production.ProductModel
        {
            ProductModelId = 2,
            Name = "Long-Sleeve Logo Jersey",
            CatalogDescription = "<catalog>jersey</catalog>",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = modifiedDate
        };

        var result = _mapper.Map<ProductModelDetailModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ProductModelId.Should().Be(2);
            result.Name.Should().Be("Long-Sleeve Logo Jersey");
            result.CatalogDescription.Should().Be("<catalog>jersey</catalog>");
            result.ModifiedDate.Should().Be(modifiedDate);
        }
    }
}
