using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the vendor detail endpoint (GET /api/v1/vendors/{id}).
/// Verifies the happy path, the structured 404 body shape produced by
/// <c>ExceptionHandlerMiddleware</c> for the <see cref="KeyNotFoundException"/> deviation, and
/// 400 on an invalid route id.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class VendorDetailEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // Unique test-only vendor id — avoids colliding with TestDataSeeder's baseline entities.
    private const int SeededVendorId = 90001;

    private async Task SeedVendorAsync()
    {
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
                Name = "Integration Test Vendor",
                AccountNumber = "ITVENDOR01",
                CreditRating = 1,
                PreferredVendorStatus = true,
                ActiveFlag = true,
                PurchasingWebServiceUrl = string.Empty,
                ModifiedDate = DateTime.UtcNow
            });

            context.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
            {
                PurchaseOrderId = SeededVendorId,
                RevisionNumber = 1,
                Status = 4,
                EmployeeId = 1,
                VendorId = SeededVendorId,
                ShipMethodId = 1,
                OrderDate = new DateTime(2014, 1, 1),
                SubTotal = 500m,
                TaxAmt = 0m,
                Freight = 0m,
                TotalDue = 500m,
                ModifiedDate = DateTime.UtcNow
            });

            await Task.CompletedTask;
        });
    }

    [Fact]
    public async Task GetVendorDetailAsync_WithAuth_ExistingVendor_Returns200WithSpendMetrics()
    {
        await SeedVendorAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/vendors/{SeededVendorId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<VendorDetailModel>(response);
        result.Should().NotBeNull();
        result!.VendorId.Should().Be(SeededVendorId);
        result.TotalSpend.Should().Be(500m);
        result.PoCount.Should().Be(1);
        result.AvgPoValue.Should().Be(500m);
    }

    [Fact]
    public async Task GetVendorDetailAsync_UnknownVendor_Returns404WithStructuredBody()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/vendors/9999999", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(json);
        document.RootElement.TryGetProperty("error", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("correlationId", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetVendorDetailAsync_InvalidId_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/vendors/0", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetVendorDetailAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"/api/v1.0/vendors/{SeededVendorId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
