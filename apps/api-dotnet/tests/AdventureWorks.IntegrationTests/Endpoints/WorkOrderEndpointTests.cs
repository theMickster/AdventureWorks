using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Production;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the Work Orders list endpoint (GET /api/v1/work-orders).
/// Verifies authentication gate, validation, and basic response shape.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class WorkOrderEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string ListUrl = "/api/v1.0/work-orders";

    [Fact]
    public async Task GetAsync_WithAuthAndValidFilter_Returns200()
    {
        var productId = await SeedWorkOrdersAsync(count: 2);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"{ListUrl}?productId={productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<WorkOrderSearchResultModel>(response);
        result.Should().NotBeNull();
        result!.Results.Should().NotBeNull();
        result.Results.Should().HaveCount(2);
        result.TotalRecords.Should().Be(2);
        result.Results!.Should().OnlyContain(x => x.ProductId == productId);
    }

    [Fact]
    public async Task GetAsync_WithPageBeyondTotal_Returns200WithEmptyResultsAndCorrectTotal()
    {
        var productId = await SeedWorkOrdersAsync(count: 2);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"{ListUrl}?productId={productId}&pageNumber=999&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<WorkOrderSearchResultModel>(response);
        result.Should().NotBeNull();
        result!.Results.Should().BeEmpty();
        result.TotalRecords.Should().Be(2);
    }

    [Fact]
    public async Task GetAsync_WithInvalidDateRange_Returns400()
    {
        var client = CreateAuthenticatedClient();

        // startDate after endDate violates Rule-02
        var response = await client.GetAsync($"{ListUrl}?startDate=2014-06-30&endDate=2014-01-01");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(ListUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<int> SeedWorkOrdersAsync(int count)
    {
        var productId = Random.Shared.Next(600000, 900000);
        var now = DateTime.UtcNow;

        await SeedAsync(async context =>
        {
            context.Products.Add(new Product
            {
                ProductId = productId,
                Name = $"Integration Test Product {productId}",
                ModifiedDate = now
            });

            for (var i = 0; i < count; i++)
            {
                context.WorkOrders.Add(new WorkOrder
                {
                    WorkOrderId = Random.Shared.Next(600000, 900000) + i,
                    ProductId = productId,
                    OrderQty = 10,
                    StockedQty = 10,
                    ScrappedQty = 0,
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddDays(-2),
                    DueDate = now.AddDays(-1),
                    ModifiedDate = now
                });
            }

            await Task.CompletedTask;
        });

        return productId;
    }
}
