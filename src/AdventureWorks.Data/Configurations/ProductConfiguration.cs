using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Product", "Production");

            builder.HasKey(p => p.ProductId);

            builder.HasOne(p => p.ProductModel)
                .WithMany(b=>b.Products)
                .HasForeignKey(p => p.ProductModelId);

            builder.HasOne(p => p.ProductSubcategory)
                .WithMany(b=>b.Products)
                .HasForeignKey(p => p.ProductSubcategoryId);

            builder.HasOne(p => p.SizeUnitMeasureCodeNavigation)
                .WithMany(b=>b.ProductSizeUnitMeasureCodeNavigation)
                .HasForeignKey(p => p.SizeUnitMeasureCode);

            builder.HasOne(p => p.WeightUnitMeasureCodeNavigation)
                .WithMany(b=>b.ProductWeightUnitMeasureCodeNavigation)
                .HasForeignKey(p => p.WeightUnitMeasureCode);
        }
    }
}
