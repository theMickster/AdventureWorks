using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ShipMethodConfiguration : IEntityTypeConfiguration<ShipMethod>
{
    public void Configure(EntityTypeBuilder<ShipMethod> builder)
    {
        builder.ToTable("ShipMethod", "Purchasing");

        builder.HasKey(a => a.ShipMethodId);

    }
}