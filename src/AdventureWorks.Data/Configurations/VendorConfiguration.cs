using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
    {
        public void Configure(EntityTypeBuilder<Vendor> builder)
        {
            builder.ToTable("Vendor", "Purchasing");

            builder.HasKey(a => a.BusinessEntityId);

            builder.HasOne(a => a.BusinessEntity)
                .WithMany(b => b.Vendors)
                .HasForeignKey(a => a.BusinessEntityId);
        }
    }
}