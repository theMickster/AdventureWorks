using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class StateProvinceConfiguration : IEntityTypeConfiguration<StateProvinceEntity>
{
    public void Configure(EntityTypeBuilder<StateProvinceEntity> builder)
    {
        builder.ToTable("AddressStateProvince", "Person");

        builder.HasKey(a => a.StateProvinceId);

        builder.HasOne(a => a.CountryRegion)
            .WithMany(b => b.StateProvinces)
            .HasForeignKey(a => a.CountryRegionCode);

        builder.HasOne(a => a.SalesTerritory)
            .WithMany(b => b.StateProvinces)
            .HasForeignKey(a => a.TerritoryId);
    }
}