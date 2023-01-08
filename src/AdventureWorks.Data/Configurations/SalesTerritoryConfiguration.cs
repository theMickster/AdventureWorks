using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class SalesTerritoryConfiguration : IEntityTypeConfiguration<SalesTerritoryEntity>
{
    public void Configure(EntityTypeBuilder<SalesTerritoryEntity> builder)
    {
        builder.ToTable("SalesTerritory", "Sales");

        builder.HasKey(a => a.TerritoryId);

        builder.HasOne(a => a.CountryRegion)
            .WithMany()
            .HasForeignKey(a => a.CountryRegionCode);
    }
}