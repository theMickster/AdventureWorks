using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.SalesOrderSaga.Persistence;

/// <summary>Maps the durable, saga-owned allocation ledger used for exact compensation.</summary>
public sealed class SalesOrderSagaInventoryAllocationConfiguration : IEntityTypeConfiguration<SalesOrderSagaInventoryAllocation>
{
    public void Configure(EntityTypeBuilder<SalesOrderSagaInventoryAllocation> builder)
    {
        builder.ToTable("SalesOrderSagaInventoryAllocation", "dbo");
        builder.HasKey(a => new { a.SagaInstanceId, a.LineNumber, a.ProductId, a.LocationId });
        builder.Property(a => a.SagaInstanceId).HasMaxLength(128);
        builder.Property(a => a.ReservedAt).HasColumnType("datetime2");
        builder.Property(a => a.ReversedAt).HasColumnType("datetime2");
    }
}
