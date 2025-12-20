using AdventureWorks.Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Functions.SalesOrderSaga.Persistence;

/// <summary>
/// Maps only the <c>Production.TransactionHistory</c> columns <c>ReserveStockActivity</c>
/// inserts. Column names/types verified against the live AdventureWorks schema —
/// <c>TransactionType</c> is <c>nchar(1)</c>; see <see cref="TransactionHistoryConstants"/> for
/// why the saga uses "S" rather than Feature 610's tech-notes wording of "SR", which does not
/// fit the column. No navigation property — this app never needs the related <c>Product</c> row.
/// </summary>
public sealed class TransactionHistoryConfiguration : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.ToTable("TransactionHistory", "Production");

        builder.HasKey(th => th.TransactionId);

        builder.Property(th => th.TransactionId).HasColumnName("TransactionID").ValueGeneratedOnAdd();
        builder.Property(th => th.ProductId).HasColumnName("ProductID");
        builder.Property(th => th.ReferenceOrderId).HasColumnName("ReferenceOrderID");
        builder.Property(th => th.ReferenceOrderLineId).HasColumnName("ReferenceOrderLineID");
        builder.Property(th => th.TransactionType).HasColumnType("nchar(1)");
        builder.Property(th => th.Quantity);
        builder.Property(th => th.ActualCost).HasColumnType("money");
        builder.Property(th => th.TransactionDate);
        builder.Property(th => th.ModifiedDate);

        builder.Ignore(th => th.Product);
    }
}
