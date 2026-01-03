using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Production;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the manufacturing KPI endpoint (GET /api/v1/manufacturing/kpis).
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ManufacturingKpisEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string KpisUrl = "/api/v1.0/manufacturing/kpis";

    [Fact]
    public async Task GetKpis_WithAuth_Returns200WithValidMetrics()
    {
        await SeedAsync(async context =>
        {
            var workOrderId = Random.Shared.Next(600000, 900000);
            context.WorkOrders.Add(new WorkOrder
            {
                WorkOrderId = workOrderId,
                ProductId = 747,
                OrderQty = 10,
                StockedQty = 8,
                ScrappedQty = 2,
                StartDate = new DateTime(2011, 6, 1),
                DueDate = new DateTime(2011, 6, 14),
                ModifiedDate = DateTime.UtcNow
            });
            await Task.CompletedTask;
        });

        var response = await CreateAuthenticatedClient().GetAsync(KpisUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<ManufacturingKpisModel>(response);
        result.Should().NotBeNull();
        result!.TotalWorkOrders.Should().BeGreaterThan(0);
        result.TotalOrdered.Should().BeGreaterThanOrEqualTo(result.TotalScrapped);
        result.TotalStocked.Should().Be(result.TotalOrdered - result.TotalScrapped);
        result.OverallYieldPct.Should().BeInRange(0m, 100m);
        result.OverallScrapPct.Should().BeInRange(0m, 100m);
    }

    [Fact]
    public async Task GetKpis_WithoutAuth_Returns401()
    {
        var response = await CreateAnonymousClient().GetAsync(KpisUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
