using System.Net;
using System.Net.Http.Json;
using AdventureWorks.SalesOrderSaga.Contracts;
using AdventureWorks.SalesOrderSaga.TestHarness;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AdventureWorks.SalesOrderSaga.TestHarness.Tests;

public sealed class HarnessEndpointTests : IClassFixture<HarnessFactory>
{
    private readonly HttpClient _client;
    public HarnessEndpointTests(HarnessFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Authorization_endpoint_validates_and_lists_request()
    {
        const int id = 900123;
        var key = SagaIds.InstanceIdFor(id);
        var payload = new PaymentAuthorizationRequest(id, 1m, "USD", key, [new SagaOrderLine(776, 1, 1m)]);
        using var invalid = await _client.PostAsJsonAsync("/authorizations", payload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/authorizations") { Content = JsonContent.Create(payload) };
        request.Headers.Add("Idempotency-Key", key);
        using var accepted = await _client.SendAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        var listed = await _client.GetFromJsonAsync<SimulatedAuthorization[]>("/test-control/authorizations", TestContext.Current.CancellationToken);
        Assert.Contains(listed!, item => item.SalesOrderId == id && item.State == AuthorizationState.Pending);
    }

    [Fact]
    public async Task Cleanup_requires_explicit_confirmation()
    {
        using var response = await _client.DeleteAsync("/test-control/fixtures", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class HarnessFactory : WebApplicationFactory<global::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development");
}
