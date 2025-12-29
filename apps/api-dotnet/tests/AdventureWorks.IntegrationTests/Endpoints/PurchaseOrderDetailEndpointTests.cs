using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the purchase order detail endpoint (GET /api/v1/purchase-orders/{id}).
/// Verifies the happy path, the null-employee-join fallback (which must still return 200, not
/// 404), the structured 404 body shape produced by <c>ExceptionHandlerMiddleware</c> for the
/// <see cref="KeyNotFoundException"/> deviation, 400 on an invalid route id, and 401 without auth.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PurchaseOrderDetailEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // Unique test-only ids — avoid colliding with TestDataSeeder's baseline entities.
    private const int SeededPurchaseOrderId = 90101;
    private const int SeededEmployeeId = 90102;
    private const int SeededNullEmployeePurchaseOrderId = 90103;

    /// <summary>
    /// Seeds the Vendor (1650), ShipMethod (5), and Product (4) rows referenced by every
    /// purchase order in this test class — required navigations for EF Core's InMemory provider,
    /// which drops the root row entirely (rather than leaving the navigation null) when a
    /// required FK doesn't resolve. Guarded with existence checks because both seed methods in
    /// this class share the collection-scoped database and must not insert duplicate keys.
    /// </summary>
    private async Task SeedVendorShipMethodAndProductAsync()
    {
        await SeedAsync(async context =>
        {
            if (!await context.Vendors.AnyAsync(v => v.BusinessEntityId == 1650))
            {
                context.Vendors.Add(new Vendor
                {
                    BusinessEntityId = 1650,
                    Name = "American Bicycles and Wheels",
                    AccountNumber = "AMERBIKE0001",
                    CreditRating = 1,
                    PreferredVendorStatus = true,
                    ActiveFlag = true,
                    PurchasingWebServiceUrl = string.Empty,
                    ModifiedDate = DateTime.UtcNow
                });
            }

            if (!await context.Set<ShipMethod>().AnyAsync(s => s.ShipMethodId == 5))
            {
                context.Set<ShipMethod>().Add(new ShipMethod
                {
                    ShipMethodId = 5,
                    Name = "CARGO TRANSPORT 5",
                    ShipBase = 3.95m,
                    ShipRate = 1.25m,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                });
            }

            if (!await context.Products.AnyAsync(p => p.ProductId == 4))
            {
                context.Products.Add(new Product
                {
                    ProductId = 4,
                    Name = "Headset Ball Bearings",
                    ProductNumber = "BE-2908",
                    MakeFlag = false,
                    FinishedGoodsFlag = false,
                    SafetyStockLevel = 1000,
                    ReorderPoint = 750,
                    StandardCost = 0m,
                    ListPrice = 0m,
                    DaysToManufacture = 0,
                    SellStartDate = new DateTime(2008, 4, 30),
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                });
            }
        });
    }

    private async Task SeedPurchaseOrderAsync()
    {
        await SeedVendorShipMethodAndProductAsync();

        await SeedAsync(async context =>
        {
            context.BusinessEntities.Add(new BusinessEntity
            {
                BusinessEntityId = SeededEmployeeId,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            });

            context.Persons.Add(new PersonEntity
            {
                BusinessEntityId = SeededEmployeeId,
                FirstName = "Integration",
                LastName = "Approver",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            });

            context.Employees.Add(new EmployeeEntity
            {
                BusinessEntityId = SeededEmployeeId,
                NationalIdnumber = "333333333",
                LoginId = "adventure-works\\integrationapprover",
                JobTitle = "Buyer",
                BirthDate = new DateTime(1980, 1, 1),
                MaritalStatus = "S",
                Gender = "M",
                HireDate = new DateTime(2010, 1, 1),
                SalariedFlag = true,
                CurrentFlag = true,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            });

            context.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
            {
                PurchaseOrderId = SeededPurchaseOrderId,
                RevisionNumber = 1,
                Status = 1,
                EmployeeId = SeededEmployeeId,
                VendorId = 1650,
                ShipMethodId = 5,
                OrderDate = new DateTime(2014, 1, 1),
                SubTotal = 171.0765m,
                TaxAmt = 13.6861m,
                Freight = 4.2769m,
                TotalDue = 189.0395m,
                ModifiedDate = DateTime.UtcNow
            });

            context.Add(new PurchaseOrderDetail
            {
                PurchaseOrderId = SeededPurchaseOrderId,
                PurchaseOrderDetailId = SeededPurchaseOrderId,
                DueDate = new DateTime(2014, 1, 15),
                OrderQty = 3,
                ProductId = 4,
                UnitPrice = 57.0255m,
                LineTotal = 171.0765m,
                ReceivedQty = 2m,
                RejectedQty = 1m,
                StockedQty = 1m,
                ModifiedDate = DateTime.UtcNow
            });

            await Task.CompletedTask;
        });
    }

    private async Task SeedPurchaseOrderWithUnresolvableEmployeeAsync()
    {
        await SeedVendorShipMethodAndProductAsync();

        await SeedAsync(async context =>
        {
            context.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
            {
                PurchaseOrderId = SeededNullEmployeePurchaseOrderId,
                RevisionNumber = 1,
                Status = 1,
                EmployeeId = 999999999,
                VendorId = 1650,
                ShipMethodId = 5,
                OrderDate = new DateTime(2014, 1, 1),
                SubTotal = 100m,
                TaxAmt = 5m,
                Freight = 1m,
                TotalDue = 106m,
                ModifiedDate = DateTime.UtcNow
            });

            await Task.CompletedTask;
        });
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_WithAuth_ExistingPurchaseOrder_Returns200WithFullDetail()
    {
        await SeedPurchaseOrderAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/purchase-orders/{SeededPurchaseOrderId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<PurchaseOrderDetailModel>(response);
        result.Should().NotBeNull();
        result!.PurchaseOrderId.Should().Be(SeededPurchaseOrderId);
        result.StatusLabel.Should().Be("Pending");
        result.ApprovingEmployeeFullName.Should().Be("Integration Approver");
        result.VendorName.Should().Be("American Bicycles and Wheels");
        result.ShipMethodName.Should().Be("CARGO TRANSPORT 5");
        result.LineItems.Should().ContainSingle();
        result.DueDate.Should().Be(new DateTime(2014, 1, 15));
        result.LineItems[0].ProductName.Should().Be("Headset Ball Bearings");
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_WithAuth_UnresolvableEmployee_Returns200WithUnknownEmployeeFallback()
    {
        await SeedPurchaseOrderWithUnresolvableEmployeeAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/purchase-orders/{SeededNullEmployeePurchaseOrderId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<PurchaseOrderDetailModel>(response);
        result.Should().NotBeNull();
        result!.ApprovingEmployeeFullName.Should().Be("Unknown employee");
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_UnknownPurchaseOrder_Returns404WithStructuredBody()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/purchase-orders/9999999", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(json);
        document.RootElement.TryGetProperty("error", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("correlationId", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_InvalidId_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/purchase-orders/0", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_WithoutAuth_Returns401()
    {
        // No seeding needed — [Authorize] rejects the request before the handler/repository run.
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"/api/v1.0/purchase-orders/{SeededPurchaseOrderId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
