namespace AdventureWorks.IntegrationTests.Setup;

/// <summary>
/// Declares the <c>"Integration Tests"</c> xUnit collection and registers
/// <see cref="CustomWebApplicationFactory"/> as its shared fixture.
/// </summary>
/// <remarks>
/// xUnit creates one <see cref="CustomWebApplicationFactory"/> instance for all test classes
/// decorated with <c>[Collection("Integration Tests")]</c>, starting the in-process test host
/// once and tearing it down after the last test in the collection completes. Test classes inherit
/// this behaviour automatically via <see cref="IntegrationTestBase"/>.
/// </remarks>
[CollectionDefinition("Integration Tests")]
[ExcludeFromCodeCoverage]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
