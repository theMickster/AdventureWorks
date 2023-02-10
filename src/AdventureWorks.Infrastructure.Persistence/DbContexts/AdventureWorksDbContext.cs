﻿using AdventureWorks.Application.Interfaces.DbContext;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.AccountInfo;
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

    public DbSet<CountryRegionEntity> CountryRegions { get; set; }

    public DbSet<Person> Persons { get; set; }

    public DbSet<SalesTerritoryEntity> SalesTerritories { get; set; }

    public DbSet<StateProvinceEntity> StateProvinces { get; set; }

    public DbSet<UserAccountEntity> UserAccounts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assemblyWithConfigurations = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfigurations);
    }
}