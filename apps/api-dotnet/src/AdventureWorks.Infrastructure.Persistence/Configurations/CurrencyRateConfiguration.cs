using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.ToTable("CurrencyRate", "Sales");

        builder.HasKey(a => a.CurrencyRateId);

        builder.HasOne(a => a.FromCurrencyCodeNavigation)
            .WithMany()
            .HasForeignKey(a => a.FromCurrencyCode);

        builder.HasOne(a => a.ToCurrencyCodeNavigation)
            .WithMany()
            .HasForeignKey(a => a.ToCurrencyCode);

    }
}