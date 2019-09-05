using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ProductModelIllustrationConfiguration : IEntityTypeConfiguration<ProductModelIllustration>
    {
        public void Configure(EntityTypeBuilder<ProductModelIllustration> builder)
        {
            builder.ToTable("ProductModelIllustration", "Production");

            builder.HasKey(a => new {a.ProductModelId, a.IllustrationId});

            builder.HasOne(a => a.ProductModel)
                .WithMany()
                .HasForeignKey(a => a.ProductModelId);

            builder.HasOne(a => a.Illustration)
                .WithMany()
                .HasForeignKey(a => a.IllustrationId);
        }
    }
}