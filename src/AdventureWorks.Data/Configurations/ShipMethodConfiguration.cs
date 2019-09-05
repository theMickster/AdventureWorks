using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ShipMethodConfiguration : IEntityTypeConfiguration<ShipMethod>
    {
        public void Configure(EntityTypeBuilder<ShipMethod> builder)
        {
            builder.ToTable("ShipMethod", "Purchasing");

            builder.HasKey(a => a.ShipMethodId);

        }
    }
}