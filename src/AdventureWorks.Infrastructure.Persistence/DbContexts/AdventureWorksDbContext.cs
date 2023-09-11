using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Entities.Shield;
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

    public DbSet<EmailAddress> EmailAddresses { get; set; }

    public DbSet<PersonEntity> Persons { get; set; }

    public DbSet<PersonTypeEntity> PersonTypes { get; set; }

    public DbSet<SalesPerson> SalesPersons { get; set; }

    public DbSet<SalesTerritoryEntity> SalesTerritories { get; set; }

    public DbSet<SecurityFunctionEntity> SecurityFunctions { get; set; }

    public DbSet<SecurityGroupEntity> SecurityGroups { get; set; }

    public DbSet<SecurityGroupSecurityFunctionEntity> SecurityGroupSecurityFunctions { get; set; }

    public DbSet<SecurityGroupSecurityRoleEntity> SecurityGroupSecurityRoles { get; set; }

    public DbSet<SecurityGroupUserAccountEntity> SecurityGroupUserAccounts { get; set; }

    public DbSet<SecurityRoleEntity> SecurityRoles { get; set; }

    public DbSet<StoreEntity> Stores { get; set; }

    public DbSet<StateProvinceEntity> StateProvinces { get; set; }

    public DbSet<UserAccountEntity> UserAccounts { get; set; }

    public DbSet<UserRefreshTokenEntity> UserRefreshTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
}