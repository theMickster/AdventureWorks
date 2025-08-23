using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Domain.Profiles.Production;

[ExcludeFromCodeCoverage]
public sealed class ProductProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public ProductProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ProductToDetailModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void All_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_product_to_detail_model_succeeds()
    {
        var products = ProductionDomainFixtures.GetMockProducts();
        var product = products[0]; // Mountain Bike with subcategory, model, and inventory

        var result = _mapper.Map<ProductDetailModel>(product);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(product.ProductId);
            result.Name.Should().Be(product.Name);
            result.ProductNumber.Should().Be(product.ProductNumber);
            result.Color.Should().Be(product.Color);
            result.ListPrice.Should().Be(product.ListPrice);
            result.StandardCost.Should().Be(product.StandardCost);
            result.SafetyStockLevel.Should().Be(product.SafetyStockLevel);
            result.ReorderPoint.Should().Be(product.ReorderPoint);
            result.DaysToManufacture.Should().Be(product.DaysToManufacture);
            result.MakeFlag.Should().Be(product.MakeFlag);
            result.FinishedGoodsFlag.Should().Be(product.FinishedGoodsFlag);
            result.SellStartDate.Should().Be(product.SellStartDate);
            result.ModifiedDate.Should().Be(product.ModifiedDate);

            result.Category.Should().NotBeNull();
            result.Category!.Name.Should().Be("Bikes");

            result.Subcategory.Should().NotBeNull();
            result.Subcategory!.Name.Should().Be("Mountain Bikes");

            result.Model.Should().NotBeNull();
            result.Model!.Name.Should().Be("Mountain-100");

            result.TotalInventoryQuantity.Should().Be(100); // 50 + 35 + 15
        }
    }

    [Fact]
    public void Map_product_to_detail_model_with_nulls_succeeds()
    {
        var products = ProductionDomainFixtures.GetMockProducts();
        var product = products[2]; // Touring Bike with null subcategory and model

        var result = _mapper.Map<ProductDetailModel>(product);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(product.ProductId);
            result.Category.Should().BeNull();
            result.Subcategory.Should().BeNull();
            result.Model.Should().BeNull();
            result.DiscontinuedDate.Should().NotBeNull();
            result.TotalInventoryQuantity.Should().Be(0);
        }
    }

    [Fact]
    public void Map_product_to_list_model_succeeds()
    {
        var products = ProductionDomainFixtures.GetMockProducts();
        var product = products[0];

        var result = _mapper.Map<ProductListModel>(product);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(product.ProductId);
            result.Name.Should().Be(product.Name);
            result.ProductNumber.Should().Be(product.ProductNumber);
            result.Color.Should().Be(product.Color);
            result.ListPrice.Should().Be(product.ListPrice);
            result.StandardCost.Should().Be(product.StandardCost);
            result.CategoryName.Should().Be("Bikes");
            result.SubcategoryName.Should().Be("Mountain Bikes");
            result.ModelName.Should().Be("Mountain-100");
            result.IsActive.Should().BeTrue();
        }
    }

    [Fact]
    public void Map_product_to_list_model_discontinued_product_succeeds()
    {
        var products = ProductionDomainFixtures.GetMockProducts();
        var product = products[2]; // Discontinued

        var result = _mapper.Map<ProductListModel>(product);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.IsActive.Should().BeFalse();
            result.CategoryName.Should().BeNull();
            result.SubcategoryName.Should().BeNull();
            result.ModelName.Should().BeNull();
        }
    }

    [Fact]
    public void Map_product_category_to_model_succeeds()
    {
        var categories = ProductionDomainFixtures.GetMockProductCategories();
        var category = categories[0];

        var result = _mapper.Map<ProductCategoryModel>(category);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(category.ProductCategoryId);
            result.Name.Should().Be(category.Name);
        }
    }

    [Fact]
    public void Map_product_subcategory_to_model_succeeds()
    {
        var subcategories = ProductionDomainFixtures.GetMockProductSubcategories();
        var subcategory = subcategories[0];

        var result = _mapper.Map<ProductSubcategoryModel>(subcategory);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.SubcategoryId.Should().Be(subcategory.ProductSubcategoryId);
            result.ProductCategoryId.Should().Be(subcategory.ProductCategoryId);
            result.Name.Should().Be(subcategory.Name);
            result.CategoryName.Should().Be("Bikes");
        }
    }

    [Fact]
    public void Map_product_inventory_to_model_succeeds()
    {
        var inventory = ProductionDomainFixtures.GetMockProductInventory(1);
        var item = inventory[0];

        var result = _mapper.Map<ProductInventoryModel>(item);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.LocationId.Should().Be(item.LocationId);
            result.LocationName.Should().Be("Tool Crib");
            result.Shelf.Should().Be(item.Shelf);
            result.Bin.Should().Be(item.Bin);
            result.Quantity.Should().Be(item.Quantity);
        }
    }

    [Fact]
    public void Map_product_list_price_history_to_model_succeeds()
    {
        var history = ProductionDomainFixtures.GetMockListPriceHistory(1);
        var item = history[0];

        var result = _mapper.Map<ProductPriceHistoryModel>(item);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ProductId.Should().Be(item.ProductId);
            result.StartDate.Should().Be(item.StartDate);
            result.EndDate.Should().Be(item.EndDate);
            result.Price.Should().Be(item.ListPrice);
            result.Type.Should().Be("list");
        }
    }

    [Fact]
    public void Map_product_cost_history_to_model_succeeds()
    {
        var history = ProductionDomainFixtures.GetMockCostHistory(1);
        var item = history[0];

        var result = _mapper.Map<ProductPriceHistoryModel>(item);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ProductId.Should().Be(item.ProductId);
            result.StartDate.Should().Be(item.StartDate);
            result.EndDate.Should().Be(item.EndDate);
            result.Price.Should().Be(item.StandardCost);
            result.Type.Should().Be("cost");
        }
    }

    [Fact]
    public void Map_product_product_photo_to_metadata_model_succeeds()
    {
        var photo = ProductionDomainFixtures.GetMockProductProductPhoto(1);

        var result = _mapper.Map<ProductPhotoMetadataModel>(photo);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.PhotoId.Should().Be(photo.ProductPhotoId);
            result.ThumbnailPhotoFileName.Should().Be("mountain_bike_thumb.gif");
            result.LargePhotoFileName.Should().Be("mountain_bike_large.gif");
            result.IsPrimary.Should().BeTrue();
        }
    }

    [Fact]
    public void Map_product_model_to_info_model_succeeds()
    {
        var models = ProductionDomainFixtures.GetMockProductModels();
        var productModel = models[0];

        var result = _mapper.Map<ProductModelInfoModel>(productModel);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ModelId.Should().Be(productModel.ProductModelId);
            result.Name.Should().Be(productModel.Name);
            result.CatalogDescription.Should().Be(productModel.CatalogDescription);
        }
    }

    [Fact]
    public void Map_product_create_model_to_product_succeeds()
    {
        var createModel = new ProductCreateModel
        {
            Name = "Test Product",
            ProductNumber = "TP-001",
            SafetyStockLevel = 100,
            ReorderPoint = 75,
            StandardCost = 500m,
            ListPrice = 999.99m,
            DaysToManufacture = 2,
            SellStartDate = new DateTime(2024, 1, 1)
        };

        var result = _mapper.Map<Product>(createModel);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Name.Should().Be(createModel.Name);
            result.ProductNumber.Should().Be(createModel.ProductNumber);
            result.SafetyStockLevel.Should().Be(createModel.SafetyStockLevel);
            result.ReorderPoint.Should().Be(createModel.ReorderPoint);
            result.StandardCost.Should().Be(createModel.StandardCost);
            result.ListPrice.Should().Be(createModel.ListPrice);
            result.DaysToManufacture.Should().Be(createModel.DaysToManufacture);
            result.SellStartDate.Should().Be(createModel.SellStartDate);
        }
    }

    [Fact]
    public void Map_product_update_model_to_product_succeeds()
    {
        var updateModel = new ProductUpdateModel
        {
            Id = 1,
            Name = "Updated Product",
            ProductNumber = "TP-002",
            SafetyStockLevel = 200,
            ReorderPoint = 150,
            StandardCost = 600m,
            ListPrice = 1099.99m,
            DaysToManufacture = 3,
            SellStartDate = new DateTime(2024, 1, 1)
        };

        var result = _mapper.Map<Product>(updateModel);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Name.Should().Be(updateModel.Name);
            result.ProductNumber.Should().Be(updateModel.ProductNumber);
            result.SafetyStockLevel.Should().Be(updateModel.SafetyStockLevel);
        }
    }
}
