using AdventureWorks.Application.Interfaces;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Infrastructure.DbContexts;

public class AdventureWorksDbContext : DbContext, IAdventureWorksDbContext
{

    public AdventureWorksDbContext(DbContextOptions<AdventureWorksDbContext> options) :base (options)
    {
    }
        
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Objects in Schema => dbo
        modelBuilder.ApplyConfiguration(new AwBuildVersionConfiguration());

        // Objects in Schema => HumanResources
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeDepartmentHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeePayHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new JobCandidateConfiguration());
        modelBuilder.ApplyConfiguration(new ShiftConfiguration());

        // Objects in Schema => Person
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new AddressTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessEntityAddressConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessEntityContactConfiguration());
        modelBuilder.ApplyConfiguration(new ContactTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CountryRegionConfiguration());
        modelBuilder.ApplyConfiguration(new EmailAddressConfiguration());
        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new PersonPhoneConfiguration());
        modelBuilder.ApplyConfiguration(new PhoneNumberTypeConfiguration());
        modelBuilder.ApplyConfiguration(new StateProvinceConfiguration());

        // Objects in Schema => Production
        modelBuilder.ApplyConfiguration(new BillOfMaterialsConfiguration());
        modelBuilder.ApplyConfiguration(new CultureConfiguration());
        modelBuilder.ApplyConfiguration(new IllustrationConfiguration());
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCostHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductDescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new ProductInventoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductListPriceHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductModelConfiguration());
        modelBuilder.ApplyConfiguration(new ProductModelIllustrationConfiguration());
        modelBuilder.ApplyConfiguration(new ProductModelProductDescriptionCultureConfiguration());
        modelBuilder.ApplyConfiguration(new ProductPhotoConfiguration());
        modelBuilder.ApplyConfiguration(new ProductProductPhotoConfiguration());
        modelBuilder.ApplyConfiguration(new ProductReviewConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSubcategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ScrapReasonConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionHistoryArchiveConfiguration());
        modelBuilder.ApplyConfiguration(new UnitMeasureConfiguration());
        modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
        modelBuilder.ApplyConfiguration(new WorkOrderRoutingConfiguration());
            
        // Objects in Schema => Purchasing
        modelBuilder.ApplyConfiguration(new ProductVendorConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseOrderDetailConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseOrderHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new ShipMethodConfiguration());
        modelBuilder.ApplyConfiguration(new VendorConfiguration());
            

        // Objects in Schema => Sales
        modelBuilder.ApplyConfiguration(new CountryRegionCurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new CreditCardConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyRateConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new PersonCreditCardConfiguration());
        modelBuilder.ApplyConfiguration(new SalesOrderDetailConfiguration());
        modelBuilder.ApplyConfiguration(new SalesOrderHeaderConfiguration());
        modelBuilder.ApplyConfiguration(new SalesOrderHeaderSalesReasonConfiguration());
        modelBuilder.ApplyConfiguration(new SalesPersonConfiguration());
        modelBuilder.ApplyConfiguration(new SalesPersonQuotaHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new SalesReasonConfiguration());
        modelBuilder.ApplyConfiguration(new SalesTaxRateConfiguration());
        modelBuilder.ApplyConfiguration(new SalesTerritoryConfiguration());
        modelBuilder.ApplyConfiguration(new SalesTerritoryHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new SalesTerritoryHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ShoppingCartItemConfiguration());
        modelBuilder.ApplyConfiguration(new SpecialOfferConfiguration());
        modelBuilder.ApplyConfiguration(new SpecialOfferProductConfiguration());
        modelBuilder.ApplyConfiguration(new StoreConfiguration());




    }
}