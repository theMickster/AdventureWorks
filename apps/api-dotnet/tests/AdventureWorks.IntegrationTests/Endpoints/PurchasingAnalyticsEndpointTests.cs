using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the purchasing analytics endpoint (GET /api/v1/purchasing/analytics).
/// Verifies the authentication gate and the basic response shape.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PurchasingAnalyticsEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string AnalyticsUrl = "/api/v1.0/purchasing/analytics";

    [Fact]
    public async Task GetAnalytics_WithAuth_Returns200()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(AnalyticsUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<PurchasingAnalyticsModel>(response);
        result.Should().NotBeNull();
        result!.ParetoData.Should().NotBeNull();

        // All four purchase order statuses are always reported, even on an empty dataset.
        result.PipelineSummary.Should().HaveCount(4);
        result.PipelineSummary.Select(x => x.StatusLabel)
            .Should().ContainInOrder("Pending", "Approved", "Rejected", "Complete");
    }

    [Fact]
    public async Task GetAnalytics_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(AnalyticsUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
