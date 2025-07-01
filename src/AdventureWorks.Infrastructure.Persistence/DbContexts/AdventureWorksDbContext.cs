using AdventureWorks.Application.PersistenceContracts.DbContext;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AdventureWorks.Infrastructure.Persistence.DbContexts;

public class AdventureWorksDbContext : DbContext, IAdventureWorksDbContext
{
    private readonly ILogger<AdventureWorksDbContext> _logger;

    public AdventureWorksDbContext(DbContextOptions<AdventureWorksDbContext> options) :base (options)
    {
        _logger = new NullLogger<AdventureWorksDbContext>();
    }

    public AdventureWorksDbContext(
        DbContextOptions<AdventureWorksDbContext> options,
        ILoggerFactory factory) : base(options)
    {
        _logger = factory.CreateLogger<AdventureWorksDbContext>();
    }
        
    public DbSet<Product> Products { get; set; }

    public DbSet<AddressEntity> Addresses { get; set; }

    public DbSet<AddressTypeEntity> AddressTypes { get; set; }

    public DbSet<BusinessEntity> BusinessEntities { get; set; }

    public DbSet<BusinessEntityAddressEntity> BusinessEntityAddresses { get; set; }

    public DbSet<BusinessEntityContactEntity> BusinessEntityContacts { get; set; }

    public DbSet<ContactTypeEntity> ContactTypes { get; set; }

    public DbSet<CountryRegionEntity> CountryRegions { get; set; }

    public DbSet<DepartmentEntity> Departments { get; set; }

    public DbSet<EmailAddressEntity> EmailAddresses { get; set; }

    public DbSet<EmployeeEntity> Employees { get; set; }

    public DbSet<PersonEntity> Persons { get; set; }

    public DbSet<PersonPhone> PersonPhones { get; set; }

    public DbSet<PersonTypeEntity> PersonTypes { get; set; }

    public DbSet<PhoneNumberTypeEntity> PhoneNumberTypes { get; set; }

    public DbSet<SalesPersonEntity> SalesPersons { get; set; }

    public DbSet<SalesTerritoryEntity> SalesTerritories { get; set; }

    public DbSet<ShiftEntity> Shifts { get; set; }
    public DbSet<StoreEntity> Stores { get; set; }

    public DbSet<StateProvinceEntity> StateProvinces { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
}