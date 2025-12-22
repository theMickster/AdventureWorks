using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Production;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

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

        var response = await client.GetAsync($"{ListUrl}?productId={productId}", TestContext.Current.CancellationToken);

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

        var response = await client.GetAsync($"{ListUrl}?productId={productId}&pageNumber=999&pageSize=10", TestContext.Current.CancellationToken);

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
        var response = await client.GetAsync($"{ListUrl}?startDate=2014-06-30&endDate=2014-01-01", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(ListUrl, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByIdAsync_WithAuth_Returns200WithFullShape()
    {
        // Real fixture: WorkOrderID 41, ProductID 518 (ML Road Seat Assembly), ScrapReasonID 7 (Handling damage)
        await SeedAsync(async context =>
        {
            context.Products.Add(new Product { ProductId = 518, Name = "ML Road Seat Assembly", ModifiedDate = DateTime.UtcNow });
            context.ScrapReasons.Add(new ScrapReason { ScrapReasonId = 7, Name = "Handling damage", ModifiedDate = DateTime.UtcNow });
            context.WorkOrders.Add(new WorkOrder
            {
                WorkOrderId = 41,
                ProductId = 518,
                OrderQty = 98,
                StockedQty = 97,
                ScrappedQty = 1,
                StartDate = new DateTime(2011, 6, 3),
                EndDate = new DateTime(2011, 6, 19),
                DueDate = new DateTime(2011, 6, 14),
                ScrapReasonId = 7,
                ModifiedDate = DateTime.UtcNow
            });
            await Task.CompletedTask;
        });

        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/work-orders/41", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var model = await DeserializeAsync<WorkOrderDetailModel>(response);
        model.Should().NotBeNull();
        model!.WorkOrderId.Should().Be(41);
        model.ProductId.Should().Be(518);
        model.ProductName.Should().Be("ML Road Seat Assembly");
        model.OrderedQty.Should().Be(98);
        model.StockedQty.Should().Be(97);
        model.ScrappedQty.Should().Be(1);
        model.IsCompletedLate.Should().BeTrue();
        model.DaysLate.Should().Be(5);
        model.ScrapReasonId.Should().Be(7);
        model.ScrapReasonName.Should().Be("Handling damage");
    }

    [Fact]
    public async Task GetByIdAsync_MissingWorkOrder_Returns404WithStructuredBody()
    {
        var client = CreateAuthenticatedClient();

        // Confirmed absent via database query
        var response = await client.GetAsync("/api/v1.0/work-orders/9999999", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.TryGetProperty("error", out _).Should().BeTrue();
        root.TryGetProperty("correlationId", out _).Should().BeTrue();
        root.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_InvalidId_Returns400(int id)
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/work-orders/{id}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByIdAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/v1.0/work-orders/1", TestContext.Current.CancellationToken);

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
