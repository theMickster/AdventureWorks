using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class ProductInventoryConfiguration : IEntityTypeConfiguration<ProductInventory>
{
    public void Configure(EntityTypeBuilder<ProductInventory> builder)
    {
        builder.ToTable("ProductInventory", "Production");

        builder.HasKey(a => new {a.ProductId, a.LocationId});

        builder.HasOne(a => a.Product)
            .WithMany(b=>b.ProductInventory)
            .HasForeignKey(a => a.ProductId);

        builder.HasOne(a => a.Location)
            .WithMany(b=> b.ProductInventory)
            .HasForeignKey(a => a.LocationId);
    }
}