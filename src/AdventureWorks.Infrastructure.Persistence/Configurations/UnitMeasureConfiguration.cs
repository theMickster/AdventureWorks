using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class UnitMeasureConfiguration : IEntityTypeConfiguration<UnitMeasure>
{
    public void Configure(EntityTypeBuilder<UnitMeasure> builder)
    {
        builder.ToTable("UnitMeasure", "Production");

        builder.HasKey(a => a.UnitMeasureCode);
    }
}