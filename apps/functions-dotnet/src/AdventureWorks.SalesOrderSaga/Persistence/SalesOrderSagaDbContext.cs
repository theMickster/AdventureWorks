using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.SalesOrderSaga.Persistence;

/// <summary>
/// Minimal EF Core context for this app's SQL needs — checking and reserving stock. Deliberately
/// not a reuse of <c>apps/api-dotnet</c>'s <c>AdventureWorksDbContext</c>: that context (and its
/// Infrastructure.Persistence project) is out of scope for the local NuGet feed this app
/// consumes, so this maps only the two tables (and only the columns) <c>CheckInventoryActivity</c>
/// and <c>ReserveStockActivity</c> touch. See Architecture Decision 4 in this app's CLAUDE.md.
/// </summary>
public sealed class SalesOrderSagaDbContext(DbContextOptions<SalesOrderSagaDbContext> options) : DbContext(options)
{
    public DbSet<ProductInventory> ProductInventories => Set<ProductInventory>();

    public DbSet<TransactionHistory> TransactionHistories => Set<TransactionHistory>();

    public DbSet<SalesOrderSagaInventoryAllocation> InventoryAllocations => Set<SalesOrderSagaInventoryAllocation>();
    public DbSet<SalesOrderSagaOutboxMessage> OutboxMessages => Set<SalesOrderSagaOutboxMessage>();
    public DbSet<SalesOrderHeader> SalesOrderHeaders => Set<SalesOrderHeader>();
    public DbSet<SalesOrderDetail> SalesOrderDetails => Set<SalesOrderDetail>();
    public DbSet<AddressEntity> Addresses => Set<AddressEntity>();
    public DbSet<ShipMethod> ShipMethods => Set<ShipMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
