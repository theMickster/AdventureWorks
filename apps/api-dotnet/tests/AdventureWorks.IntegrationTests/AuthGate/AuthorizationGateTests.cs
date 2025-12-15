using AdventureWorks.IntegrationTests.Setup;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.AuthGate;

public sealed class AuthorizationGateTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetStore_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"/api/v1.0/stores/{TestConstants.SeededStoreId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetStore_Authenticated_WithSeededStore_Returns200WithStoreData()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/stores/{TestConstants.SeededStoreId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.EnumerateObject()
            .Any(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue("StoreModel must include the store's Id in the response");
    }

    [Fact]
    public async Task PostStore_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();
        var body = new { name = "Test Store" };

        var response = await client.PostAsJsonAsync("/api/v1.0/stores", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostStore_Authenticated_WithValidModel_Returns201WithLocation()
    {
        var client = CreateAuthenticatedClient();
        var body = new { name = $"IT-{Guid.NewGuid():N}"[..20] };

        var response = await client.PostAsJsonAsync("/api/v1.0/stores", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull(
            "a 201 Created response must include a Location header");
    }

    /// <summary>
    /// Read-only endpoints that require authentication (US-991). Anonymous requests must return 401;
    /// authenticated requests must reach the handler (any non-401 status, since most of these entities
    /// have no InMemory seed data and legitimately resolve to 404).
    /// </summary>
    public static IEnumerable<object[]> AuthorizedReadEndpoints()
    {
        yield return new object[] { "/api/v1.0/products/1" };
        yield return new object[] { "/api/v1.0/products?pageNumber=1&pageSize=10" };
        yield return new object[] { "/api/v1.0/products/1/inventory" };
        yield return new object[] { "/api/v1.0/products/1/price-history" };
        yield return new object[] { "/api/v1.0/departments/1" };
        yield return new object[] { "/api/v1.0/departments" };
        yield return new object[] { $"/api/v1.0/addresses/{TestConstants.SeededStoreId}" };
        yield return new object[] { "/api/v1.0/territories/1" };
        yield return new object[] { "/api/v1.0/territories" };
        yield return new object[] { "/api/v1.0/shifts/1" };
        yield return new object[] { "/api/v1.0/shifts" };
        yield return new object[] { "/api/v1.0/product-models" };
        yield return new object[] { "/api/v1.0/products/1/reviews" };
        yield return new object[] { "/api/v1.0/products/1/reviews/statistics" };
    }

    /// <summary>
    /// Reference/dropdown endpoints intentionally marked <c>[AllowAnonymous]</c> (US-991, first
    /// usage in the codebase). Anonymous requests must not be challenged with 401.
    /// </summary>
    public static IEnumerable<object[]> AnonymousReadEndpoints()
    {
        yield return new object[] { "/api/v1.0/products/categories" };
        yield return new object[] { "/api/v1.0/products/subcategories" };
        yield return new object[] { "/api/v1.0/phoneNumberTypes" };
        yield return new object[] { "/api/v1.0/countries" };
        yield return new object[] { "/api/v1.0/states" };
    }

    [Theory]
    [MemberData(nameof(AuthorizedReadEndpoints))]
    public async Task AuthorizedReadEndpoint_Anonymous_Returns401(string requestUri)
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(requestUri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(AuthorizedReadEndpoints))]
    public async Task AuthorizedReadEndpoint_Authenticated_DoesNotReturn401(string requestUri)
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(requestUri);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAddress_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();
        var body = new { id = TestConstants.SeededStoreId };

        var response = await client.PutAsJsonAsync($"/api/v1.0/addresses/{TestConstants.SeededStoreId}", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAddress_Authenticated_DoesNotReturn401()
    {
        var client = CreateAuthenticatedClient();
        var body = new { id = TestConstants.SeededStoreId };

        var response = await client.PutAsJsonAsync($"/api/v1.0/addresses/{TestConstants.SeededStoreId}", body);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(AnonymousReadEndpoints))]
    public async Task AllowAnonymousReadEndpoint_Anonymous_DoesNotReturn401(string requestUri)
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(requestUri);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
