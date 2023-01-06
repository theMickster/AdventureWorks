using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ProductVendorConfiguration : IEntityTypeConfiguration<ProductVendor>
    {
        public void Configure(EntityTypeBuilder<ProductVendor> builder)
        {
            builder.ToTable("ProductVendor", "Purchasing");

            builder.HasKey(a => new {a.ProductId, a.BusinessEntityId});

            builder.HasOne(a => a.Product)
                .WithMany(b=>b.ProductVendors)
                .HasForeignKey(a => a.ProductId);

            builder.HasOne(a => a.BusinessEntity)
                .WithMany(b => b.ProductVendors)
                .HasForeignKey(a => a.BusinessEntityId);

            builder.HasOne(a => a.UnitMeasureCodeNavigation)
                .WithMany(b => b.ProductVendors)
                .HasForeignKey(a => a.UnitMeasureCode);
        }
    }
}