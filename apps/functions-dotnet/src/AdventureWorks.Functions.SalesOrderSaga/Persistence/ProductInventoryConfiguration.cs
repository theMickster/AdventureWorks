using AdventureWorks.Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Functions.SalesOrderSaga.Persistence;

/// <summary>
/// Maps only the <c>Production.ProductInventory</c> columns <c>CheckInventoryActivity</c> and
/// <c>ReserveStockActivity</c> touch — <c>ProductId</c>, <c>LocationId</c> (composite key),
/// <c>Quantity</c>, and <c>rowguid</c>. Column names/types verified against the live
/// AdventureWorks schema (<c>ProductId int</c>, <c>LocationId smallint</c>,
/// <c>Quantity smallint</c>) rather than assumed from <c>apps/api-dotnet</c>'s
/// <c>ProductInventoryConfiguration</c>, which this app isn't reusing. No navigation
/// properties — this app never needs <c>Product</c> or <c>Location</c> details, only the
/// quantity. <c>Rowguid</c> is mapped (not ignored) and marked as a concurrency token: two
/// sales order sagas racing to reserve the same <c>ProductId</c>/<c>LocationId</c> row would
/// otherwise both read the same <c>Quantity</c>, both compute their own decrement, and the
/// second <c>SaveChangesAsync</c> would silently overwrite the first's decrement — a classic
/// lost update that <c>ReserveStockActivityCore.RunAsync</c>'s <c>BeginTransactionAsync</c>
/// alone does <em>not</em> prevent (it only makes one saga instance's own batch atomic, not
/// isolated from a concurrent instance). With <c>Rowguid</c> as a concurrency token,
/// <c>ReserveStockActivityCore</c> assigns it a new value on every decrement, so a losing
/// writer's <c>SaveChangesAsync</c> throws <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>
/// instead of silently corrupting the row.
/// </summary>
public sealed class ProductInventoryConfiguration : IEntityTypeConfiguration<ProductInventory>
{
    public void Configure(EntityTypeBuilder<ProductInventory> builder)
    {
        builder.ToTable("ProductInventory", "Production");

        builder.HasKey(pi => new { pi.ProductId, pi.LocationId });

        builder.Property(pi => pi.ProductId).HasColumnName("ProductID");
        builder.Property(pi => pi.LocationId).HasColumnName("LocationID");
        builder.Property(pi => pi.Quantity);
        builder.Property(pi => pi.Rowguid).HasColumnName("rowguid").IsConcurrencyToken();

        builder.Ignore(pi => pi.Shelf);
        builder.Ignore(pi => pi.Bin);
        builder.Ignore(pi => pi.ModifiedDate);
        builder.Ignore(pi => pi.Location);
        builder.Ignore(pi => pi.Product);
    }
}
