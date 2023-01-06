using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ProductListPriceHistoryConfiguration : IEntityTypeConfiguration<ProductListPriceHistory>
    {
        public void Configure(EntityTypeBuilder<ProductListPriceHistory> builder)
        {
            builder.ToTable("ProductListPriceHistory", "Production");

            builder.HasKey(a => new {a.ProductId, a.StartDate});

            builder.HasOne(a => a.Product)
                .WithMany(b=>b.ProductListPriceHistory)
                .HasForeignKey(a => a.ProductId);

        }
    }
}