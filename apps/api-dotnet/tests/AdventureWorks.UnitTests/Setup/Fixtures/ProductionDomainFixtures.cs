using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.UnitTests.Setup.Fixtures;

[ExcludeFromCodeCoverage]
public static class ProductionDomainFixtures
{
    public static readonly DateTime DefaultModifiedDate = new(2011, 11, 11, 11, 11, 11, DateTimeKind.Utc);

    public static List<Product> GetMockProducts()
    {
        return
        [
            new Product
            {
                ProductId = 1,
                Name = "Mountain Bike",
                ProductNumber = "BK-M82S-38",
                MakeFlag = true,
                FinishedGoodsFlag = true,
                Color = "Silver",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 1912.1544m,
                ListPrice = 3399.99m,
                Size = "38",
                SizeUnitMeasureCode = "CM",
                WeightUnitMeasureCode = "LB",
                Weight = 20.77m,
                DaysToManufacture = 4,
                ProductLine = "M",
                Class = "H",
                Style = "U",
                ProductSubcategoryId = 1,
                ProductModelId = 19,
                SellStartDate = new DateTime(2011, 5, 31),
                SellEndDate = null,
                DiscontinuedDate = null,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DefaultModifiedDate,
                ProductSubcategory = GetMockProductSubcategories().First(s => s.ProductSubcategoryId == 1),
                ProductModel = GetMockProductModels().First(m => m.ProductModelId == 19),
                ProductProductPhotos = [],
                ProductInventory = GetMockProductInventory(1)
            },
            new Product
            {
                ProductId = 2,
                Name = "Road Bike",
                ProductNumber = "BK-R93R-44",
                MakeFlag = true,
                FinishedGoodsFlag = true,
                Color = "Red",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 868.6342m,
                ListPrice = 1431.50m,
                Size = "44",
                SizeUnitMeasureCode = "CM",
                WeightUnitMeasureCode = "LB",
                Weight = 17.35m,
                DaysToManufacture = 4,
                ProductLine = "R",
                Class = "M",
                Style = "U",
                ProductSubcategoryId = 2,
                ProductModelId = 25,
                SellStartDate = new DateTime(2011, 5, 31),
                SellEndDate = null,
                DiscontinuedDate = null,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DefaultModifiedDate,
                ProductSubcategory = GetMockProductSubcategories().First(s => s.ProductSubcategoryId == 2),
                ProductModel = GetMockProductModels().First(m => m.ProductModelId == 25),
                ProductProductPhotos = [],
                ProductInventory = []
            },
            new Product
            {
                ProductId = 3,
                Name = "Touring Bike",
                ProductNumber = "BK-T79Y-50",
                MakeFlag = true,
                FinishedGoodsFlag = true,
                Color = "Yellow",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 1481.9379m,
                ListPrice = 2384.07m,
                Size = "50",
                SizeUnitMeasureCode = "CM",
                WeightUnitMeasureCode = "LB",
                Weight = 25.13m,
                DaysToManufacture = 4,
                ProductLine = "T",
                Class = "H",
                Style = "U",
                ProductSubcategoryId = 3,
                ProductModelId = 30,
                SellStartDate = new DateTime(2011, 5, 31),
                SellEndDate = new DateTime(2013, 5, 29),
                DiscontinuedDate = new DateTime(2013, 6, 1),
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DefaultModifiedDate,
                ProductSubcategory = null,
                ProductModel = null,
                ProductProductPhotos = [],
                ProductInventory = []
            }
        ];
    }

    public static List<ProductCategory> GetMockProductCategories()
    {
        return
        [
            new ProductCategory { ProductCategoryId = 1, Name = "Bikes", Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductCategory { ProductCategoryId = 2, Name = "Components", Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductCategory { ProductCategoryId = 3, Name = "Clothing", Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductCategory { ProductCategoryId = 4, Name = "Accessories", Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate }
        ];
    }

    public static List<ProductSubcategory> GetMockProductSubcategories()
    {
        var categories = GetMockProductCategories();
        return
        [
            new ProductSubcategory { ProductSubcategoryId = 1, ProductCategoryId = 1, Name = "Mountain Bikes", ProductCategory = categories[0], Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductSubcategory { ProductSubcategoryId = 2, ProductCategoryId = 1, Name = "Road Bikes", ProductCategory = categories[0], Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductSubcategory { ProductSubcategoryId = 3, ProductCategoryId = 1, Name = "Touring Bikes", ProductCategory = categories[0], Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate }
        ];
    }

    public static List<ProductModel> GetMockProductModels()
    {
        return
        [
            new ProductModel { ProductModelId = 19, Name = "Mountain-100", CatalogDescription = "Top-of-the-line mountain bike.", Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductModel { ProductModelId = 25, Name = "Road-150", CatalogDescription = "Performance road bike.", Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductModel { ProductModelId = 30, Name = "Touring-1000", CatalogDescription = "Touring bike for long rides.", Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate }
        ];
    }

    public static List<ProductInventory> GetMockProductInventory(int productId)
    {
        return
        [
            new ProductInventory { ProductId = productId, LocationId = 1, Shelf = "A", Bin = 1, Quantity = 50, Location = new Location { LocationId = 1, Name = "Tool Crib", CostRate = 0, Availability = 0, ModifiedDate = DefaultModifiedDate }, Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductInventory { ProductId = productId, LocationId = 6, Shelf = "B", Bin = 5, Quantity = 35, Location = new Location { LocationId = 6, Name = "Miscellaneous Storage", CostRate = 0, Availability = 0, ModifiedDate = DefaultModifiedDate }, Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate },
            new ProductInventory { ProductId = productId, LocationId = 50, Shelf = "C", Bin = 10, Quantity = 15, Location = new Location { LocationId = 50, Name = "Subassembly", CostRate = 0, Availability = 0, ModifiedDate = DefaultModifiedDate }, Rowguid = Guid.NewGuid(), ModifiedDate = DefaultModifiedDate }
        ];
    }

    public static List<ProductListPriceHistory> GetMockListPriceHistory(int productId)
    {
        return
        [
            new ProductListPriceHistory { ProductId = productId, StartDate = new DateTime(2011, 5, 31), EndDate = new DateTime(2012, 5, 29), ListPrice = 3399.99m, ModifiedDate = DefaultModifiedDate },
            new ProductListPriceHistory { ProductId = productId, StartDate = new DateTime(2012, 5, 30), EndDate = null, ListPrice = 3578.27m, ModifiedDate = DefaultModifiedDate }
        ];
    }

    public static List<ProductCostHistory> GetMockCostHistory(int productId)
    {
        return
        [
            new ProductCostHistory { ProductId = productId, StartDate = new DateTime(2011, 5, 31), EndDate = new DateTime(2012, 5, 29), StandardCost = 1912.1544m, ModifiedDate = DefaultModifiedDate },
            new ProductCostHistory { ProductId = productId, StartDate = new DateTime(2012, 5, 30), EndDate = null, StandardCost = 1898.0944m, ModifiedDate = DefaultModifiedDate }
        ];
    }

    public static ProductProductPhoto GetMockProductProductPhoto(int productId)
    {
        return new ProductProductPhoto
        {
            ProductId = productId,
            ProductPhotoId = 100,
            Primary = true,
            ModifiedDate = DefaultModifiedDate,
            ProductPhoto = new ProductPhoto
            {
                ProductPhotoId = 100,
                ThumbnailPhotoFileName = "mountain_bike_thumb.gif",
                LargePhotoFileName = "mountain_bike_large.gif",
                ModifiedDate = DefaultModifiedDate
            }
        };
    }
}
