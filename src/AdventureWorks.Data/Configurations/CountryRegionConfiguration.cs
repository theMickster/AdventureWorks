using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class CountryRegionConfiguration : IEntityTypeConfiguration<CountryRegion>
{
    public void Configure(EntityTypeBuilder<CountryRegion> builder)
    {
        builder.ToTable("CountryRegion", "Person");
        builder.HasKey(a => a.CountryRegionCode);
    }
}