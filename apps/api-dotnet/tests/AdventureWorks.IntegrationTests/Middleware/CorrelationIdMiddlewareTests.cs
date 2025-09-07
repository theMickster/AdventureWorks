using AdventureWorks.IntegrationTests.Setup;

namespace AdventureWorks.IntegrationTests.Middleware;

public sealed class CorrelationIdMiddlewareTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Request_WithoutCorrelationId_ResponseContainsGuidCorrelationId()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/health");

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue();
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Guid.TryParse(correlationId, out _).Should().BeTrue(
            "the generated correlation ID must be a valid GUID");
    }

    [Fact]
    public async Task Request_WithCorrelationId_ResponseEchoesSameValue()
    {
        const string expectedCorrelationId = "test-correlation-123";
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", expectedCorrelationId);

        var response = await client.GetAsync("/health");

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue();
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        correlationId.Should().Be(expectedCorrelationId);
    }
}
