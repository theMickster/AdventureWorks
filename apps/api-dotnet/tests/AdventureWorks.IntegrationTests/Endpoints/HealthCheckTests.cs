using AdventureWorks.IntegrationTests.Setup;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

public sealed class HealthCheckTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetHealth_Anonymous_Returns200WithHealthyStatus()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Healthy");
    }
}
