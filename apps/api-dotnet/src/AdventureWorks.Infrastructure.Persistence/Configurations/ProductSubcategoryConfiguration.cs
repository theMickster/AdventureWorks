using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ProductSubcategoryConfiguration : IEntityTypeConfiguration<ProductSubcategory>
{
    public void Configure(EntityTypeBuilder<ProductSubcategory> builder)
    {
        builder.ToTable("ProductSubcategory", "Production");

        builder.HasKey(p => p.ProductSubcategoryId);

        builder.HasOne(a => a.ProductCategory)
            .WithMany(b => b.ProductSubcategories)
            .HasForeignKey(a => a.ProductCategoryId);
    }
}