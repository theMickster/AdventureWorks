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
}
