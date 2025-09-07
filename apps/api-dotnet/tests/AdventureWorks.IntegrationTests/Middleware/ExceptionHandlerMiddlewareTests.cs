using AdventureWorks.IntegrationTests.Setup;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdventureWorks.IntegrationTests.Middleware;

public sealed class ExceptionHandlerMiddlewareTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Post_WithInvalidModel_Returns400WithValidationErrorShape()
    {
        var client = CreateAuthenticatedClient();
        var invalidBody = new { };

        var response = await client.PostAsJsonAsync("/api/v1.0/stores", invalidBody);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.ValueKind.Should().Be(JsonValueKind.Array,
            "ValidationException errors are serialized as a JSON array");
        var firstError = root[0];
        firstError.TryGetProperty("propertyName", out _).Should().BeTrue();
        firstError.TryGetProperty("errorCode", out _).Should().BeTrue();
        firstError.TryGetProperty("errorMessage", out _).Should().BeTrue();
        firstError.TryGetProperty("correlationId", out _).Should().BeTrue();
    }

}
