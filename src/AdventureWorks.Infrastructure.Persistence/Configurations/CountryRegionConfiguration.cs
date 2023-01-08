using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class CountryRegionConfiguration : IEntityTypeConfiguration<CountryRegionEntity>
{
    public void Configure(EntityTypeBuilder<CountryRegionEntity> builder)
    {
        builder.ToTable("CountryRegion", "Person");

        builder.HasKey(a => a.CountryRegionCode);

    }
}