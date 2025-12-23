using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the vendor purchase order history endpoint
/// (GET /api/v1/vendors/{id}/purchase-orders). Verifies the happy path, filter validation
/// (status range, date range), default pagination, and the 404 for an unknown vendor.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class VendorPurchaseOrderHistoryEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // Unique test-only vendor id — avoids colliding with TestDataSeeder's baseline entities.
    private const int SeededVendorId = 90002;

    // The factory/database is shared across every test in this class (xUnit collection fixture),
    // so seeding must run exactly once — re-seeding the same vendor id from multiple test methods
    // would violate the InMemory provider's primary-key uniqueness.
    private static bool _seeded;

    private async Task SeedVendorWithPurchaseOrdersAsync()
    {
        if (_seeded)
        {
            return;
        }

        _seeded = true;

        await SeedAsync(async context =>
        {
            context.BusinessEntities.Add(new BusinessEntity
            {
                BusinessEntityId = SeededVendorId,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            });

            context.Vendors.Add(new Vendor
            {
                BusinessEntityId = SeededVendorId,
                Name = "Integration Test PO History Vendor",
                AccountNumber = "ITVENDOR02",
                CreditRating = 1,
                PreferredVendorStatus = true,
                ActiveFlag = true,
                PurchasingWebServiceUrl = string.Empty,
                ModifiedDate = DateTime.UtcNow
            });

            context.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
            {
                PurchaseOrderId = SeededVendorId * 10 + 1,
                RevisionNumber = 1,
                Status = 1,
                EmployeeId = 1,
                VendorId = SeededVendorId,
                ShipMethodId = 1,
                OrderDate = new DateTime(2014, 1, 1),
                SubTotal = 100m,
                TaxAmt = 0m,
                Freight = 0m,
                TotalDue = 100m,
                ModifiedDate = DateTime.UtcNow
            });

            context.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
            {
                PurchaseOrderId = SeededVendorId * 10 + 2,
                RevisionNumber = 1,
                Status = 4,
                EmployeeId = 1,
                VendorId = SeededVendorId,
                ShipMethodId = 1,
                OrderDate = new DateTime(2014, 6, 15),
                SubTotal = 200m,
                TaxAmt = 0m,
                Freight = 0m,
                TotalDue = 200m,
                ModifiedDate = DateTime.UtcNow
            });

            await Task.CompletedTask;
        });
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_WithAuth_ExistingVendor_Returns200WithPaginatedShape()
    {
        await SeedVendorWithPurchaseOrdersAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            $"/api/v1.0/vendors/{SeededVendorId}/purchase-orders", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<PurchaseOrderSearchResultModel>(response);
        result.Should().NotBeNull();
        result!.Results.Should().HaveCount(2);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_FilterByStatus_ReturnsOnlyMatchingOrders()
    {
        await SeedVendorWithPurchaseOrdersAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            $"/api/v1.0/vendors/{SeededVendorId}/purchase-orders?status=4", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<PurchaseOrderSearchResultModel>(response);
        result!.Results.Should().ContainSingle();
        result.Results![0].StatusLabel.Should().Be("Complete");
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_FilterByDateRange_ReturnsOnlyMatchingOrders()
    {
        await SeedVendorWithPurchaseOrdersAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            $"/api/v1.0/vendors/{SeededVendorId}/purchase-orders?startDate=2014-05-01&endDate=2014-12-31",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<PurchaseOrderSearchResultModel>(response);
        result!.Results.Should().ContainSingle();
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_InvalidStatus_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            $"/api/v1.0/vendors/{SeededVendorId}/purchase-orders?status=9", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_InvalidDateRange_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            $"/api/v1.0/vendors/{SeededVendorId}/purchase-orders?startDate=2014-12-31&endDate=2014-01-01",
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_UnknownVendor_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            "/api/v1.0/vendors/9999999/purchase-orders", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_InvalidId_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/vendors/0/purchase-orders", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVendorPurchaseOrdersAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(
            $"/api/v1.0/vendors/{SeededVendorId}/purchase-orders", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
