namespace AdventureWorks.IntegrationTests.Setup;

/// <summary>
/// Shared constants used across integration test setup and test classes.
/// </summary>
/// <remarks>
/// Entity IDs correspond to the records inserted by <see cref="TestDataSeeder"/> and are stable
/// for the lifetime of the collection run. Claim values are used by <see cref="TestAuthHandler"/>
/// to build the synthetic principal for authenticated requests.
/// </remarks>
[ExcludeFromCodeCoverage]
internal static class TestConstants
{
    /// <summary>Object ID (<c>oid</c>) claim placed in the test principal by <see cref="TestAuthHandler"/>.</summary>
    internal const string DefaultObjectId = "b3362804-959e-4fcf-ba0a-854804543415";

    /// <summary>UPN (<c>preferred_username</c>) claim placed in the test principal.</summary>
    internal const string DefaultUpn = "testuser@adventureworks-test.local";

    /// <summary>Display name (<c>name</c>) claim placed in the test principal.</summary>
    internal const string DefaultDisplayName = "Integration Test User";

    /// <summary>
    /// <c>BusinessEntityId</c> of the <c>StoreEntity</c> seeded by <see cref="TestDataSeeder"/>.
    /// Use this ID in tests that read or update an existing store.
    /// </summary>
    internal const int SeededStoreId = 292;

    /// <summary>
    /// <c>SalesPersonId</c> assigned to the seeded store. Matches the <c>BusinessEntityId</c>
    /// of the sales person record inserted by <see cref="TestDataSeeder"/>.
    /// </summary>
    internal const int SeededSalesPersonId = 279;

    /// <summary>
    /// <c>BusinessEntityId</c> of the <c>PersonEntity</c> seeded by <see cref="TestDataSeeder"/>.
    /// Use this ID in tests that read or update email addresses for an existing person.
    /// </summary>
    internal const int SeededPersonId = 1001;

    /// <summary>
    /// <c>PhoneNumberTypeId</c> of the <c>PhoneNumberTypeEntity</c> seeded by <see cref="TestDataSeeder"/>.
    /// Use this ID in tests that create or update phone numbers for an existing person.
    /// </summary>
    internal const int SeededPhoneNumberTypeId = 1;
}
