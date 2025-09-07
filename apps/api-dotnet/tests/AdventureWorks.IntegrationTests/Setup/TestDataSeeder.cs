using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;

namespace AdventureWorks.IntegrationTests.Setup;

/// <summary>
/// Inserts the minimum set of entities required for integration tests to run against a
/// predictable, known-good data state.
/// </summary>
/// <remarks>
/// Called once per factory instance from <see cref="CustomWebApplicationFactory.CreateHost"/>
/// before any test in the collection executes. The seeded IDs are exposed via
/// <see cref="TestConstants"/> so test classes can reference them without magic numbers.
/// </remarks>
[ExcludeFromCodeCoverage]
internal static class TestDataSeeder
{
    private static readonly DateTime AuditDate = new(2021, 11, 11, 11, 15, 7, DateTimeKind.Utc);

    /// <summary>
    /// Seeds a <c>BusinessEntity</c> and a linked <c>StoreEntity</c> into the supplied context.
    /// </summary>
    /// <param name="context">The InMemory <see cref="AdventureWorksDbContext"/> to populate.</param>
    /// <remarks>
    /// The <c>BusinessEntity</c> row is inserted first because <c>StoreEntity</c> carries a
    /// foreign key to it. Both rows use <see cref="TestConstants.SeededStoreId"/> as their
    /// primary key so tests can retrieve the store via a known, stable ID.
    /// </remarks>
    internal static async Task SeedAsync(AdventureWorksDbContext context)
    {
        context.BusinessEntities.Add(new BusinessEntity
        {
            BusinessEntityId = TestConstants.SeededStoreId,
            Rowguid = new Guid("a22517e3-848d-4ebe-b9d9-7437f3432304"),
            ModifiedDate = AuditDate
        });

        context.Stores.Add(new StoreEntity
        {
            BusinessEntityId = TestConstants.SeededStoreId,
            Name = "Next-Door Bike Store",
            SalesPersonId = TestConstants.SeededSalesPersonId,
            Rowguid = new Guid("a22517e3-848d-4ebe-b9d9-7437f3432300"),
            ModifiedDate = AuditDate
        });

        await context.SaveChangesAsync();
    }
}
