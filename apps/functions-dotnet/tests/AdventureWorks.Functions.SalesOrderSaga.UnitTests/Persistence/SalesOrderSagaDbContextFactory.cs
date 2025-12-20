using AdventureWorks.Functions.SalesOrderSaga.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdventureWorks.Functions.SalesOrderSaga.UnitTests.Persistence;

/// <summary>
/// Builds an EF Core InMemory-backed <see cref="SalesOrderSagaDbContext"/> per test — a fresh,
/// uniquely named database so tests never share state. Used by activity tests that exercise
/// real LINQ query/update logic against <see cref="SalesOrderSagaDbContext"/>'s configured
/// entity mappings, rather than mocking <c>DbSet</c> (impractical for `Sum`/`Where` queries).
/// <see cref="InMemoryEventId.TransactionIgnoredWarning"/> is suppressed — the InMemory provider
/// has no real transactions, but <c>ReserveStockActivityCore</c> still calls
/// <c>Database.BeginTransactionAsync</c> as it would against SQL Server in production.
/// </summary>
internal static class SalesOrderSagaDbContextFactory
{
    public static SalesOrderSagaDbContext Create() => Create(Guid.NewGuid().ToString());

    /// <summary>
    /// Builds a context against an explicitly named InMemory database — used when a test needs
    /// two separate <see cref="SalesOrderSagaDbContext"/> instances (simulating two concurrent
    /// activity invocations, each with their own DbContext/connection) to share the same
    /// underlying store.
    /// </summary>
    public static SalesOrderSagaDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<SalesOrderSagaDbContext>()
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new SalesOrderSagaDbContext(options);
    }
}
