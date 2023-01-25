using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Application.Interfaces.DbContext;

public interface IAdventureWorksDbContext
{
    DbSet<Product> Products { get; set; }

    DbSet<AddressEntity> Addresses { get; set; }

    DbSet<CountryRegionEntity> CountryRegions { get; set; }

    DbSet<StateProvinceEntity> StateProvinces { get; set; }
}