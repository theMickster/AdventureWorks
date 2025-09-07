using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.Setup;

/// <summary>
/// Base class for all integration test classes in the <c>"Integration Tests"</c> collection.
/// Provides access to the shared <see cref="CustomWebApplicationFactory"/> and helper methods
/// for creating HTTP clients, seeding test data, and deserializing responses.
/// </summary>
/// <remarks>
/// Inheriting classes automatically participate in the xUnit collection fixture: the same
/// <see cref="CustomWebApplicationFactory"/> instance — and therefore the same InMemory
/// database — is reused across all test classes in the collection. Tests that write data
/// via <see cref="SeedAsync"/> should use values that do not conflict with the baseline
/// data inserted by <see cref="TestDataSeeder"/>.
/// </remarks>
[Collection("Integration Tests")]
[ExcludeFromCodeCoverage]
public abstract class IntegrationTestBase
{
    /// <summary>The shared test host factory for the current collection run.</summary>
    protected readonly CustomWebApplicationFactory Factory;

    /// <summary>
    /// Initializes a new instance of <see cref="IntegrationTestBase"/> with the collection-scoped
    /// factory injected by xUnit.
    /// </summary>
    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Returns an <see cref="HttpClient"/> whose requests are authenticated by
    /// <see cref="TestAuthHandler"/>. Use for testing endpoints protected by <c>[Authorize]</c>.
    /// </summary>
    protected HttpClient CreateAuthenticatedClient() => Factory.CreateAuthenticatedClient();

    /// <summary>
    /// Returns an <see cref="HttpClient"/> that sends the <see cref="TestAuthHandler.AnonymousHeader"/>
    /// on every request. Use for verifying that protected endpoints return HTTP 401.
    /// </summary>
    protected HttpClient CreateAnonymousClient() => Factory.CreateAnonymousClient();

    /// <summary>
    /// Seeds additional data into the collection-scoped InMemory database.
    /// </summary>
    /// <param name="seed">
    /// A delegate that adds entities to the supplied <see cref="AdventureWorksDbContext"/>.
    /// <see cref="AdventureWorksDbContext.SaveChangesAsync()"/> is called automatically after
    /// the delegate returns.
    /// </param>
    /// <remarks>
    /// Data written here persists for the remainder of the collection run. Use unique,
    /// test-specific IDs to avoid conflicting with baseline data or with other tests.
    /// </remarks>
    protected async Task SeedAsync(Func<AdventureWorksDbContext, Task> seed)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AdventureWorksDbContext>();
        await seed(ctx);
        await ctx.SaveChangesAsync();
    }

    /// <summary>
    /// Reads the response body and deserializes it as <typeparamref name="T"/> using
    /// case-insensitive property matching to accommodate the API's camelCase JSON output.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="response">The HTTP response whose content will be read.</param>
    /// <returns>The deserialized value, or <c>null</c> if the body is empty or does not match.</returns>
    protected static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}
