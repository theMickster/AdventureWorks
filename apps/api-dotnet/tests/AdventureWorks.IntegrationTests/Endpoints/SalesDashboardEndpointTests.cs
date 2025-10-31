using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Sales;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the Sales Dashboard endpoint (GET /api/v1.0/sales/dashboard).
/// Verifies authentication gate and basic response shape.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SalesDashboardEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string DashboardUrl = "/api/v1.0/sales/dashboard";

    [Fact]
    public async Task GetDashboard_WithAuth_Returns200()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(DashboardUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SalesDashboardModel>(response);
        result.Should().NotBeNull();
        result!.TopPerformers.Should().NotBeNull();
        result.TerritoryBreakdown.Should().NotBeNull();
        result.MonthlySalesTrend.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboard_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(DashboardUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
