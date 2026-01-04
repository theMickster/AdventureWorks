using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Production;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the manufacturing quality scorecard endpoint.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ManufacturingQualityScorecardEndpointTests(CustomWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    private const string ScorecardUrl = "/api/v1.0/manufacturing/quality-scorecard";

    [Fact]
    public async Task GetAsync_WithAuth_Returns200WithScorecardShape()
    {
        var productId = Random.Shared.Next(600000, 900000);
        var workOrderId = Random.Shared.Next(600000, 900000);
        var scrapReasonId = (short)Random.Shared.Next(20, 100);

        await SeedAsync(async context =>
        {
            context.Products.Add(new Product
            {
                ProductId = productId,
                Name = "Quality Scorecard Test Product",
                ModifiedDate = DateTime.UtcNow
            });
            context.ScrapReasons.Add(new ScrapReason
            {
                ScrapReasonId = scrapReasonId,
                Name = "Quality Scorecard Test Reason",
                ModifiedDate = DateTime.UtcNow
            });
            context.WorkOrders.Add(new WorkOrder
            {
                WorkOrderId = workOrderId,
                ProductId = productId,
                OrderQty = 1000,
                StockedQty = 900,
                ScrappedQty = 100,
                ScrapReasonId = scrapReasonId,
                StartDate = new DateTime(2011, 6, 1),
                DueDate = new DateTime(2011, 6, 14),
                ModifiedDate = DateTime.UtcNow
            });
            await Task.CompletedTask;
        });

        var response = await CreateAuthenticatedClient()
            .GetAsync(ScorecardUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var scorecard = await DeserializeAsync<ManufacturingQualityScorecardModel>(response);
        scorecard.Should().NotBeNull();
        scorecard!.Top5ByScrapped.Should().HaveCountLessThanOrEqualTo(5);
        scorecard.Bottom5ByYield.Should().HaveCountLessThanOrEqualTo(5);
        scorecard.ScrapReasonBreakdown.Should().NotBeNull();
        scorecard.Top5ByScrapped.Should().Contain(product => product.ProductId == productId);
        scorecard.ScrapReasonBreakdown.Should().Contain(reason => reason.ScrapReasonId == scrapReasonId);
    }

    [Fact]
    public async Task GetAsync_WithoutAuth_Returns401()
    {
        var response = await CreateAnonymousClient()
            .GetAsync(ScorecardUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
