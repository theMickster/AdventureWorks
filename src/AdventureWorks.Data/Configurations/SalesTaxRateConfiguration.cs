using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class SalesTaxRateConfiguration : IEntityTypeConfiguration<SalesTaxRateEntity>
{
    public void Configure(EntityTypeBuilder<SalesTaxRateEntity> builder)
    {
        builder.ToTable("SalesTaxRate", "Sales");

        builder.HasKey(a => a.SalesTaxRateId);

        builder.HasOne(a => a.StateProvince)
            .WithMany()
            .HasForeignKey(a => a.StateProvinceId);
    }
}