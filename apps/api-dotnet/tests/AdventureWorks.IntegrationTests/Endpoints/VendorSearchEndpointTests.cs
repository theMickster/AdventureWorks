using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the vendor list endpoint (GET /api/v1/vendors).
/// Verifies authentication gate, validation, default pagination, and basic response shape.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class VendorSearchEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string VendorsUrl = "/api/v1.0/vendors";

    [Fact]
    public async Task GetAsync_WithAuth_Returns200WithPaginatedShape()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(VendorsUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<VendorSearchResultModel>(response);
        result.Should().NotBeNull();
        result!.Results.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_WithAuth_DefaultsToPageSize25()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(VendorsUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<VendorSearchResultModel>(response);
        result!.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task GetAsync_WithoutAuth_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(VendorsUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAsync_WithInvalidCreditRating_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"{VendorsUrl}?creditRating=9");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task GetAsync_WithValidCreditRating_Returns200(byte validCreditRating)
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"{VendorsUrl}?creditRating={validCreditRating}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAsync_PageBeyondTotal_Returns200WithEmptyResultsAndCorrectTotalCount()
    {
        var client = CreateAuthenticatedClient();

        var firstPageResponse = await client.GetAsync(VendorsUrl);
        var firstPage = await DeserializeAsync<VendorSearchResultModel>(firstPageResponse);

        var response = await client.GetAsync($"{VendorsUrl}?pageNumber=999999&pageSize=25");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<VendorSearchResultModel>(response);
        result!.Results.Should().BeEmpty();
        result.TotalRecords.Should().Be(firstPage!.TotalRecords);
    }

    [Fact]
    public async Task GetAsync_WithPreferredVendorStatusFilter_Returns200()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"{VendorsUrl}?preferredVendorStatus=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAsync_WithActiveFlagFilter_Returns200()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"{VendorsUrl}?activeFlag=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
