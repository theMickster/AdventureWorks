using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ProductProductPhotoConfiguration : IEntityTypeConfiguration<ProductProductPhoto>
{
    public void Configure(EntityTypeBuilder<ProductProductPhoto> builder)
    {
        builder.ToTable("ProductProductPhoto", "Production");

        builder.HasKey(a => new {a.ProductId, a.ProductPhotoId });

        builder.HasOne(a => a.Product)
            .WithMany(b=>b.ProductProductPhotos)
            .HasForeignKey(a => a.ProductId);

        builder.HasOne(a => a.ProductPhoto)
            .WithMany(b=> b.ProductProductPhotos)
            .HasForeignKey(a => a.ProductPhotoId);
    }
}