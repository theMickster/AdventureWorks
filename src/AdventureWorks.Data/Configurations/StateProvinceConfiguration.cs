using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class StateProvinceConfiguration : IEntityTypeConfiguration<StateProvince>
    {
        public void Configure(EntityTypeBuilder<StateProvince> builder)
        {
            builder.ToTable("StateProvince", "Person");

            builder.HasKey(a => a.StateProvinceId);

            builder.HasOne(a => a.CountryRegionCodeNavigation)
                .WithMany()
                .HasForeignKey(a => a.CountryRegionCode);

            builder.HasOne(a => a.Territory)
                .WithMany()
                .HasForeignKey(a => a.TerritoryId);
        }
    }
}