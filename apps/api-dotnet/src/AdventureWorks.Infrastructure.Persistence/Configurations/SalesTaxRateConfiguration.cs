using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

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