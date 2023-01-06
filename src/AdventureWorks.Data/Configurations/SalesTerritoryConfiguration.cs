using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class SalesTerritoryConfiguration : IEntityTypeConfiguration<SalesTerritory>
    {
        public void Configure(EntityTypeBuilder<SalesTerritory> builder)
        {
            builder.ToTable("SalesTerritory", "Sales");

            builder.HasKey(a => a.TerritoryId);

            builder.HasOne(a => a.CountryRegionCodeNavigation)
                .WithMany()
                .HasForeignKey(a => a.CountryRegionCode);
        }
    }
}