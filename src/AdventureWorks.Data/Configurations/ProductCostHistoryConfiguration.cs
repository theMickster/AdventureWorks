using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class ProductCostHistoryConfiguration : IEntityTypeConfiguration<ProductCostHistory>
{
    public void Configure(EntityTypeBuilder<ProductCostHistory> builder)
    {
        builder.ToTable("ProductCostHistory", "Production");

        builder.HasKey(a => new {a.ProductId, a.StartDate});

        builder.HasOne(a => a.Product)
            .WithMany(b=>b.ProductCostHistory)
            .HasForeignKey(a => a.ProductId);
    }
}