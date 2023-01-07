using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations;

public class SalesOrderHeaderConfiguration : IEntityTypeConfiguration<SalesOrderHeader>
{
    public void Configure(EntityTypeBuilder<SalesOrderHeader> builder)
    {
        builder.ToTable("SalesOrderHeader", "Sales");

        builder.HasKey(a => a.SalesOrderId);

        builder.HasOne(a => a.Customer)
            .WithMany(b => b.SalesOrderHeaders)
            .HasForeignKey(a => a.CustomerId);

        builder.HasOne(a => a.SalesPerson)
            .WithMany(b=>b.SalesOrderHeaders)
            .HasForeignKey(a => a.SalesPersonId);

        builder.HasOne(a => a.Territory)
            .WithMany(b=> b.SalesOrderHeaders)
            .HasForeignKey(a => a.TerritoryId);

        builder.HasOne(a => a.BillToAddressEntity)
            .WithMany(b => b.SalesOrderHeaderBillToAddresses)
            .HasForeignKey(a => a.BillToAddressId);

        builder.HasOne(a => a.ShipToAddressEntity)
            .WithMany(b => b.SalesOrderHeaderShipToAddress)
            .HasForeignKey(a => a.ShipToAddressId);

        builder.HasOne(a => a.ShipMethod)
            .WithMany(b=> b.SalesOrderHeaders)
            .HasForeignKey(a => a.ShipMethodId);

        builder.HasOne(a => a.CreditCard)
            .WithMany(b=>b.SalesOrderHeaders)
            .HasForeignKey(a => a.CreditCardId);

        builder.HasOne(a => a.CurrencyRate)
            .WithMany(b=> b.SalesOrderHeaders)
            .HasForeignKey(a => a.CurrencyRateId);

    }
}