using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Sales;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the Sales Orders analytics endpoint (POST /api/v1.0/sales-orders/analytics).
/// Verifies authentication and customer-scope filter validation against other public sales APIs.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SalesOrderAnalyticsEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string AnalyticsUrl = "/api/v1.0/sales-orders/analytics";
    private const string CustomersUrl = "/api/v1.0/customers";
    private const string SalesOrdersUrl = "/api/v1.0/sales-orders";
    private const int SeededAnalyticsCustomerId = 90401;
    private const int SeededAnalyticsStoreId = 90402;
    private const string SeededAnalyticsStoreName = "Analytics Test Cycles";
    private const string SeededAnalyticsAccountNumber = "AW00090401";
    private const int SeededDistractorCustomerId = 90403;
    private const int SeededDistractorStoreId = 90404;
    private const string SeededDistractorStoreName = "Analytics Distractor Bikes";
    private const string SeededDistractorAccountNumber = "AW00090403";
    private const int SeededAnalyticsOrderIdOne = 90410;
    private const int SeededAnalyticsOrderIdTwo = 90411;
    private const int SeededDistractorOrderId = 90412;
    private const int SeededCustomerOrderCount = 2;
    private const decimal SeededCustomerTotalRevenue = 300m;
    private static readonly DateTime SeededCustomerMostRecentOrderDate = new(2026, 4, 20);

    /// <summary>
    /// Seeds a customer-scoped dataset dedicated to this test class.
    /// Guarded because the collection-scoped InMemory database is shared across all test methods
    /// in this class, so repeat calls must be idempotent instead of throwing duplicate-key errors.
    /// </summary>
    private async Task SeedAnalyticsCustomersAsync()
    {
        await SeedAsync(async context =>
        {
            if (await context.Set<CustomerEntity>().AnyAsync(c => c.CustomerId == SeededAnalyticsCustomerId))
            {
                return;
            }

            context.Stores.AddRange(
            [
                new StoreEntity
                {
                    BusinessEntityId = SeededAnalyticsStoreId,
                    Name = SeededAnalyticsStoreName,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                },
                new StoreEntity
                {
                    BusinessEntityId = SeededDistractorStoreId,
                    Name = SeededDistractorStoreName,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                }
            ]);

            context.Set<CustomerEntity>().AddRange(
            [
                new CustomerEntity
                {
                    CustomerId = SeededAnalyticsCustomerId,
                    StoreId = SeededAnalyticsStoreId,
                    AccountNumber = SeededAnalyticsAccountNumber,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                },
                new CustomerEntity
                {
                    CustomerId = SeededDistractorCustomerId,
                    StoreId = SeededDistractorStoreId,
                    AccountNumber = SeededDistractorAccountNumber,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                }
            ]);

            context.SalesOrderHeaders.AddRange(
            [
                CreateSalesOrder(
                    SeededAnalyticsOrderIdOne,
                    SeededAnalyticsCustomerId,
                    SeededAnalyticsAccountNumber,
                    new DateTime(2026, 2, 5),
                    subTotal: 90m,
                    taxAmt: 9m,
                    freight: 1m),
                CreateSalesOrder(
                    SeededAnalyticsOrderIdTwo,
                    SeededAnalyticsCustomerId,
                    SeededAnalyticsAccountNumber,
                    SeededCustomerMostRecentOrderDate,
                    subTotal: 180m,
                    taxAmt: 18m,
                    freight: 2m),
                CreateSalesOrder(
                    SeededDistractorOrderId,
                    SeededDistractorCustomerId,
                    SeededDistractorAccountNumber,
                    new DateTime(2026, 5, 3),
                    subTotal: 270m,
                    taxAmt: 27m,
                    freight: 3m)
            ]);
        });
    }

    private static SalesOrderHeader CreateSalesOrder(
        int salesOrderId,
        int customerId,
        string accountNumber,
        DateTime orderDate,
        decimal subTotal,
        decimal taxAmt,
        decimal freight)
    {
        return new SalesOrderHeader
        {
            SalesOrderId = salesOrderId,
            RevisionNumber = 1,
            OrderDate = orderDate,
            DueDate = orderDate.AddDays(7),
            ShipDate = orderDate.AddDays(2),
            Status = 5,
            OnlineOrderFlag = true,
            SalesOrderNumber = $"SO{salesOrderId}",
            PurchaseOrderNumber = $"PO{salesOrderId}",
            AccountNumber = accountNumber,
            CustomerId = customerId,
            BillToAddressId = 1,
            ShipToAddressId = 1,
            ShipMethodId = 1,
            CreditCardApprovalCode = $"APP{salesOrderId}",
            SubTotal = subTotal,
            TaxAmt = taxAmt,
            Freight = freight,
            TotalDue = subTotal + taxAmt + freight,
            Comment = "Seeded integration-test sales order",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithAuthAndCustomerFilter_MatchesCustomerScopedPublicApiData()
    {
        await SeedAnalyticsCustomersAsync();
        var client = CreateAuthenticatedClient();

        var customerDetailResponse = await client.GetAsync(
            $"{CustomersUrl}/{SeededAnalyticsCustomerId}",
            TestContext.Current.CancellationToken);

        var filteredListResponse = await client.GetAsync(
            $"{SalesOrdersUrl}?customerId={SeededAnalyticsCustomerId}&pageNumber=1&pageSize=50",
            TestContext.Current.CancellationToken);

        var analyticsResponse = await client.PostAsJsonAsync(
            AnalyticsUrl,
            new { customerId = SeededAnalyticsCustomerId },
            TestContext.Current.CancellationToken);

        customerDetailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        filteredListResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        analyticsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var customerDetail = await DeserializeAsync<CustomerDetailModel>(customerDetailResponse);
        var filteredList = await DeserializeAsync<SalesOrderSearchResultModel>(filteredListResponse);
        var analytics = await DeserializeAsync<SalesOrderAnalyticsModel>(analyticsResponse);

        customerDetail.Should().NotBeNull();
        filteredList.Should().NotBeNull();
        analytics.Should().NotBeNull();

        var detail = customerDetail ?? throw new InvalidOperationException("Expected customer detail payload.");
        var list = filteredList ?? throw new InvalidOperationException("Expected filtered sales-order list payload.");
        var analyticsModel = analytics ?? throw new InvalidOperationException("Expected analytics payload.");

        detail.CustomerId.Should().Be(SeededAnalyticsCustomerId);
        detail.DisplayName.Should().Be(SeededAnalyticsStoreName);
        detail.OrderCount.Should().Be(SeededCustomerOrderCount);
        detail.TotalSpend.Should().Be(SeededCustomerTotalRevenue);
        detail.LastOrderDate.Should().HaveValue();
        detail.LastOrderDate.Should().Be(SeededCustomerMostRecentOrderDate);

        list.TotalRecords.Should().Be(
            detail.OrderCount,
            "because the filtered sales-order list and customer detail endpoints should describe the same customer-scoped order slice");
        list.TotalRecords.Should().Be(SeededCustomerOrderCount);
        list.Results.Should().NotBeNullOrEmpty();
        list.Results.Should().HaveCount(SeededCustomerOrderCount);

        var listResults = list.Results ?? throw new InvalidOperationException("Expected filtered sales-order results.");
        listResults.Should().OnlyContain(
            order => order.CustomerName == detail.DisplayName,
            $"because customerId={SeededAnalyticsCustomerId} should restrict the list to the seeded analytics customer");
        listResults.Select(order => order.SalesOrderId)
            .Should().BeEquivalentTo([SeededAnalyticsOrderIdOne, SeededAnalyticsOrderIdTwo]);

        analyticsModel.OrderCount.Should().Be(
            list.TotalRecords,
            "because analytics must aggregate the same customer-scoped order slice as the filtered list endpoint");
        analyticsModel.TotalRevenue.Should().Be(
            detail.TotalSpend,
            "because analytics revenue should match the customer detail lifetime spend for the same customer");
        analyticsModel.TotalRevenue.Should().Be(SeededCustomerTotalRevenue);
        analyticsModel.MonthlyTrend.Should().NotBeNullOrEmpty();

        var lastOrderDate = detail.LastOrderDate.GetValueOrDefault();
        var mostRecentTrendMonth = analyticsModel.MonthlyTrend.Last();
        mostRecentTrendMonth.Year.Should().Be(
            lastOrderDate.Year,
            "because the analytics trend should include the customer's most recent order month");
        mostRecentTrendMonth.Month.Should().Be(
            lastOrderDate.Month,
            "because the analytics trend should include the customer's most recent order month");
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync(
            AnalyticsUrl,
            new { },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithInvalidCustomerId_Returns400WithRule09()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            AnalyticsUrl,
            new { customerId = 0 },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        document.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        document.RootElement.EnumerateArray()
            .Select(error => error.GetProperty("errorCode").GetString())
            .Should().Contain("Rule-09");
    }
}
