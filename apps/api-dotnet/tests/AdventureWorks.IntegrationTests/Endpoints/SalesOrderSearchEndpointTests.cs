using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Sales;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the Search Sales Orders endpoint (POST /api/v1.0/sales-orders/search).
/// Verifies authentication gate, validation, and basic response shape.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SalesOrderSearchEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string SearchUrl = "/api/v1.0/sales-orders/search";

    [Fact]
    public async Task SearchAsync_WithAuthAndValidBody_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var body = new { accountNumber = "10-4020-000676", status = 5 };

        var response = await client.PostAsJsonAsync(SearchUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SalesOrderSearchResultModel>(response);
        result.Should().NotBeNull();
        result!.Results.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithAuthAndEmptyBody_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var body = new { };

        var response = await client.PostAsJsonAsync(SearchUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SalesOrderSearchResultModel>(response);
        result.Should().NotBeNull();
        result!.Results.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();
        var body = new { };

        var response = await client.PostAsJsonAsync(SearchUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchAsync_WithAuthAndInvalidAccountNumberLength_Returns400()
    {
        var client = CreateAuthenticatedClient();
        // AccountNumber exceeds the 15-character limit validated by Rule-08
        var body = new { accountNumber = "1234567890123456" };

        var response = await client.PostAsJsonAsync(SearchUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
