using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Domain.Entities;
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

    public DbSet<CountryRegionEntity> CountryRegions { get; set; }

    public DbSet<StateProvinceEntity> StateProvinces { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
}