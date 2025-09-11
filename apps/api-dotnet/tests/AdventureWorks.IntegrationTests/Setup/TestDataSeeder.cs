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
    /// Seeds baseline entities into the supplied context.
    /// </summary>
    /// <param name="context">The InMemory <see cref="AdventureWorksDbContext"/> to populate.</param>
    /// <remarks>
    /// The <c>BusinessEntity</c> row is inserted first because dependent entities carry
    /// foreign keys to it. Seeded IDs are exposed via <see cref="TestConstants"/>.
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

        context.BusinessEntities.Add(new BusinessEntity
        {
            BusinessEntityId = TestConstants.SeededPersonId,
            Rowguid = new Guid(TestConstants.DefaultObjectId),
            IsEntraUser = true,
            ModifiedDate = AuditDate
        });

        context.PersonTypes.Add(new PersonTypeEntity
        {
            PersonTypeId = 1,
            PersonTypeCode = "EM",
            PersonTypeName = "Employee",
            PersonTypeDescription = "Integration test employee type"
        });

        context.Persons.Add(new PersonEntity
        {
            BusinessEntityId = TestConstants.SeededPersonId,
            PersonTypeId = 1,
            FirstName = "Integration",
            LastName = "TestPerson",
            Rowguid = new Guid("c4473905-a60f-4fda-db1b-965905654526"),
            ModifiedDate = AuditDate
        });

        context.PhoneNumberTypes.Add(new PhoneNumberTypeEntity
        {
            PhoneNumberTypeId = TestConstants.SeededPhoneNumberTypeId,
            Name = "Cell",
            ModifiedDate = AuditDate
        });

        await context.SaveChangesAsync();
    }
}
