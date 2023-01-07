using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class SpecialOfferProductConfiguration : IEntityTypeConfiguration<SpecialOfferProduct>
{
    public void Configure(EntityTypeBuilder<SpecialOfferProduct> builder)
    {
        builder.ToTable("SpecialOfferProduct", "Sales");

        builder.HasKey(a => new {a.SpecialOfferId, a.ProductId});

        builder.HasOne(a => a.Product)
            .WithMany(b=>b.SpecialOfferProducts)
            .HasForeignKey(a => a.ProductId);

        builder.HasOne(a => a.SpecialOffer)
            .WithMany(b=>b.SpecialOfferProducts)
            .HasForeignKey(a => a.SpecialOfferId);
    }
}