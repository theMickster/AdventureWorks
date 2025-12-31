using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Sales;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the customer detail endpoint (GET /api/v1/customers/{id}). Verifies
/// the happy path for both a store and an individual customer, the structured 404 body shape
/// produced by <c>ExceptionHandlerMiddleware</c> for the <see cref="KeyNotFoundException"/>
/// deviation, 400 on an invalid route id, 401 without auth, and that <c>ltvRank</c> agrees
/// between the list endpoint and this detail endpoint for the same customer.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CustomerDetailEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // Unique test-only ids — avoid colliding with TestDataSeeder's baseline entities.
    private const int SeededStoreCustomerId = 90201;
    private const int SeededStoreId = 90202;
    private const int SeededIndividualCustomerId = 90203;
    private const int SeededPersonId = 90204;
    private const int SeededZeroOrderCustomerId = 90205;
    private const int SeededZeroOrderPersonId = 90206;

    /// <summary>
    /// Guarded with existence checks — the InMemory database is shared across every test method
    /// in this class (collection-scoped factory), so a second invocation must be a no-op rather
    /// than throw on a duplicate key.
    /// </summary>
    private async Task SeedCustomersAsync()
    {
        await SeedAsync(async context =>
        {
            if (await context.Set<CustomerEntity>().AnyAsync(c => c.CustomerId == SeededStoreCustomerId))
            {
                return;
            }

            context.Stores.Add(new StoreEntity
            {
                BusinessEntityId = SeededStoreId,
                Name = "Integration Test Bikes",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            });

            context.Persons.AddRange(new List<PersonEntity>
            {
                new()
                {
                    BusinessEntityId = SeededPersonId,
                    FirstName = "Integration",
                    LastName = "Customer",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                },
                new()
                {
                    BusinessEntityId = SeededZeroOrderPersonId,
                    FirstName = "Zero",
                    LastName = "Orders",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                }
            });

            context.Set<CustomerEntity>().AddRange(new List<CustomerEntity>
            {
                new()
                {
                    CustomerId = SeededStoreCustomerId,
                    StoreId = SeededStoreId,
                    AccountNumber = "AW00090201",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                },
                new()
                {
                    CustomerId = SeededIndividualCustomerId,
                    PersonId = SeededPersonId,
                    AccountNumber = "AW00090203",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                },
                new()
                {
                    CustomerId = SeededZeroOrderCustomerId,
                    PersonId = SeededZeroOrderPersonId,
                    AccountNumber = "AW00090205",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                }
            });

            context.SalesOrderHeaders.AddRange(new List<SalesOrderHeader>
            {
                new()
                {
                    SalesOrderId = 90210,
                    CustomerId = SeededStoreCustomerId,
                    SalesOrderNumber = "SO90210",
                    AccountNumber = "AW00090201",
                    OrderDate = new DateTime(2026, 3, 1),
                    DueDate = new DateTime(2026, 3, 8),
                    TotalDue = 1000m,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                },
                new()
                {
                    SalesOrderId = 90211,
                    CustomerId = SeededIndividualCustomerId,
                    SalesOrderNumber = "SO90211",
                    AccountNumber = "AW00090203",
                    OrderDate = new DateTime(2026, 2, 1),
                    DueDate = new DateTime(2026, 2, 8),
                    TotalDue = 400m,
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DateTime.UtcNow
                }
            });
        });
    }

    [Fact]
    public async Task GetCustomerDetailAsync_WithAuth_StoreCustomer_Returns200WithFullDetail()
    {
        await SeedCustomersAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/customers/{SeededStoreCustomerId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<CustomerDetailModel>(response);
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(SeededStoreCustomerId);
        result.CustomerType.Should().Be("Store");
        result.DisplayName.Should().Be("Integration Test Bikes");
        result.StoreId.Should().Be(SeededStoreId);
        result.StoreName.Should().Be("Integration Test Bikes");
        result.FirstName.Should().BeNull();
        result.LastName.Should().BeNull();
        result.TotalSpend.Should().Be(1000m);
        result.OrderCount.Should().Be(1);
        result.AvgOrderValue.Should().Be(1000m);
    }

    [Fact]
    public async Task GetCustomerDetailAsync_WithAuth_IndividualCustomer_Returns200WithFullDetail()
    {
        await SeedCustomersAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/customers/{SeededIndividualCustomerId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<CustomerDetailModel>(response);
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(SeededIndividualCustomerId);
        result.CustomerType.Should().Be("Individual");
        result.DisplayName.Should().Be("Integration Customer");
        result.StoreId.Should().BeNull();
        result.FirstName.Should().Be("Integration");
        result.LastName.Should().Be("Customer");
        result.TotalSpend.Should().Be(400m);
        result.OrderCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCustomerDetailAsync_WithAuth_ZeroOrderCustomer_Returns200WithZeroAvgOrderValue()
    {
        await SeedCustomersAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/customers/{SeededZeroOrderCustomerId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<CustomerDetailModel>(response);
        result.Should().NotBeNull();
        result!.OrderCount.Should().Be(0);
        result.TotalSpend.Should().Be(0m);
        result.AvgOrderValue.Should().Be(0m);
        result.IsInactive.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomerDetailAsync_UnknownCustomer_Returns404WithStructuredBody()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/customers/9999999", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(json);
        document.RootElement.TryGetProperty("error", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("correlationId", out _).Should().BeTrue();
        document.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomerDetailAsync_InvalidId_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/customers/0", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomerDetailAsync_WithoutAuth_Returns401()
    {
        // No seeding needed — [Authorize] rejects the request before the handler/repository run.
        var client = CreateAnonymousClient();

        var response = await client.GetAsync($"/api/v1.0/customers/{SeededStoreCustomerId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerDetailAsync_LtvRank_MatchesListEndpoint_ForTheSameCustomer()
    {
        await SeedCustomersAsync();
        var client = CreateAuthenticatedClient();

        var detailResponse = await client.GetAsync($"/api/v1.0/customers/{SeededStoreCustomerId}", TestContext.Current.CancellationToken);
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await DeserializeAsync<CustomerDetailModel>(detailResponse);

        var listResponse = await client.GetAsync("/api/v1.0/customers?pageNumber=1&pageSize=50", TestContext.Current.CancellationToken);
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await DeserializeAsync<CustomerSearchResultModel>(listResponse);

        var listItem = list!.Results!.Single(r => r.CustomerId == SeededStoreCustomerId);

        detail!.LtvRank.Should().Be(listItem.LtvRank, "because rank must be identical between the list and detail endpoints");
    }
}
