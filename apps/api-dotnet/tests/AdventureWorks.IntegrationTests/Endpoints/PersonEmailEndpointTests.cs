using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Person;
using System.Net;
using System.Net.Http.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

public sealed class PersonEmailEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static readonly string BaseUrl = $"/api/v1.0/persons/{TestConstants.SeededPersonId}/emails";
    private const string NonExistentPersonUrl = "/api/v1.0/persons/999999/emails";

    [Fact]
    public async Task GetEmails_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEmails_Authenticated_ForSeededPerson_Returns200WithEmptyList()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var emails = await DeserializeAsync<List<PersonEmailModel>>(response);
        emails.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEmails_Authenticated_ForNonExistentPerson_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(NonExistentPersonUrl);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostEmail_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();
        var body = new { emailAddress = "anon@example.com" };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostEmail_Authenticated_WithValidModel_Returns201()
    {
        var client = CreateAuthenticatedClient();
        var uniqueEmail = $"test-{Guid.NewGuid():N}@example.com";
        var body = new { emailAddress = uniqueEmail };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostEmail_Authenticated_ForNonExistentPerson_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var body = new { emailAddress = "test@example.com" };

        var response = await client.PostAsJsonAsync(NonExistentPersonUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostEmail_Authenticated_WithInvalidEmail_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var body = new { emailAddress = "not-an-email" };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutEmail_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();
        var body = new { emailAddress = "updated@example.com" };

        var response = await client.PutAsJsonAsync($"{BaseUrl}/1", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutEmail_Authenticated_ForNonExistentEmail_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var body = new { emailAddress = "updated@example.com" };

        var response = await client.PutAsJsonAsync($"{BaseUrl}/99999", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteEmail_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.DeleteAsync($"{BaseUrl}/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteEmail_Authenticated_ForNonExistentEmail_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.DeleteAsync($"{BaseUrl}/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
