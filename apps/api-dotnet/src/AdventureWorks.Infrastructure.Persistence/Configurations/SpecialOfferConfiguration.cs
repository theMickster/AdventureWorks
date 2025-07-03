using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class SpecialOfferConfiguration : IEntityTypeConfiguration<SpecialOffer>
{
    public void Configure(EntityTypeBuilder<SpecialOffer> builder)
    {
        builder.ToTable("SpecialOffer", "Sales");

        builder.HasKey(a => a.SpecialOfferId);
    }
}