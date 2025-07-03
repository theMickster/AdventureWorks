using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.HumanResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<ShiftEntity>
{
    public void Configure(EntityTypeBuilder<ShiftEntity> builder)
    {
        builder.ToTable("Shift", "HumanResources");
        builder.HasKey(a => a.ShiftId);
    }
}