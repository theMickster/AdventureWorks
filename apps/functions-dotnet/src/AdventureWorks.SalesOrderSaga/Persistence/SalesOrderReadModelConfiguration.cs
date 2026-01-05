using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.SalesOrderSaga.Persistence;

/// <summary>Maps only the sales-order and shipping columns required to confirm an approved saga.</summary>
public sealed class SalesOrderHeaderSagaConfiguration : IEntityTypeConfiguration<SalesOrderHeader>
{
    public void Configure(EntityTypeBuilder<SalesOrderHeader> builder)
    {
        builder.ToTable("SalesOrderHeader", "Sales");
        builder.HasKey(o => o.SalesOrderId);
        builder.HasMany(o => o.SalesOrderDetails).WithOne().HasForeignKey(d => d.SalesOrderId);
        builder.HasOne(o => o.ShipToAddressEntity).WithMany().HasForeignKey(o => o.ShipToAddressId);
        builder.HasOne(o => o.ShipMethod).WithMany().HasForeignKey(o => o.ShipMethodId);
        builder.Ignore(o => o.SalesOrderHeaderSalesReasons);
        builder.Ignore(o => o.BillToAddressEntity);
        builder.Ignore(o => o.CreditCard);
        builder.Ignore(o => o.CurrencyRate);
        builder.Ignore(o => o.CustomerEntity);
        builder.Ignore(o => o.SalesPerson);
        builder.Ignore(o => o.TerritoryEntity);
    }
}

public sealed class SalesOrderDetailSagaConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
    {
        builder.ToTable("SalesOrderDetail", "Sales");
        builder.HasKey(o => new { o.SalesOrderId, o.SalesOrderDetailId });
        builder.Ignore(o => o.SalesOrder);
        builder.Ignore(o => o.SpecialOfferProduct);
        builder.Ignore(o => o.Product);
    }
}

public sealed class AddressSagaConfiguration : IEntityTypeConfiguration<AddressEntity>
{
    public void Configure(EntityTypeBuilder<AddressEntity> builder)
    {
        builder.ToTable("Address", "Person");
        builder.HasKey(a => a.AddressId);
        builder.Ignore(a => a.StateProvince);
        builder.Ignore(a => a.SalesOrderHeaderBillToAddresses);
        builder.Ignore(a => a.SalesOrderHeaderShipToAddress);
    }
}

public sealed class ShipMethodSagaConfiguration : IEntityTypeConfiguration<ShipMethod>
{
    public void Configure(EntityTypeBuilder<ShipMethod> builder)
    {
        builder.ToTable("ShipMethod", "Purchasing");
        builder.HasKey(s => s.ShipMethodId);
        builder.Ignore(s => s.PurchaseOrderHeaders);
        builder.Ignore(s => s.SalesOrderHeaders);
    }
}
