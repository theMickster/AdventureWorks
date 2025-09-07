using AdventureWorks.IntegrationTests.Setup;
using System.Net;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

public sealed class VersionEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetVersion_Anonymous_Returns200WithJsonMetadata()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/version");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }
}
