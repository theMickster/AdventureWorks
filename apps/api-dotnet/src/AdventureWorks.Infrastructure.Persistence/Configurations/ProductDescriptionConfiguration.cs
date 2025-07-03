using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class ProductDescriptionConfiguration : IEntityTypeConfiguration<ProductDescription>
{
    public void Configure(EntityTypeBuilder<ProductDescription> builder)
    {
        builder.ToTable("ProductDescription", "Production");

        builder.HasKey(a => a.ProductDescriptionId);
    }
}