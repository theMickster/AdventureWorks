using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Person;
using System.Net;
using System.Net.Http.Json;

namespace AdventureWorks.IntegrationTests.Endpoints;

public sealed class PersonPhoneEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static readonly string BaseUrl = $"/api/v1.0/persons/{TestConstants.SeededPersonId}/phones";
    private const string NonExistentPersonUrl = "/api/v1.0/persons/999999/phones";

    [Fact]
    public async Task GetPhones_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPhones_Authenticated_ForSeededPerson_Returns200WithEmptyList()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var phones = await DeserializeAsync<List<PersonPhoneModel>>(response);
        phones.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPhones_Authenticated_ForNonExistentPerson_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(NonExistentPersonUrl);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostPhone_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();
        var body = new { phoneNumber = "555-000-0001", phoneNumberTypeId = TestConstants.SeededPhoneNumberTypeId };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostPhone_Authenticated_WithValidModel_Returns201()
    {
        var client = CreateAuthenticatedClient();
        var uniquePhone = $"555-{Guid.NewGuid():N}"[..14];
        var body = new { phoneNumber = uniquePhone, phoneNumberTypeId = TestConstants.SeededPhoneNumberTypeId };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostPhone_Authenticated_ForNonExistentPerson_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var body = new { phoneNumber = "555-000-0099", phoneNumberTypeId = TestConstants.SeededPhoneNumberTypeId };

        var response = await client.PostAsJsonAsync(NonExistentPersonUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostPhone_Authenticated_WithEmptyPhoneNumber_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var body = new { phoneNumber = string.Empty, phoneNumberTypeId = TestConstants.SeededPhoneNumberTypeId };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostPhone_Authenticated_WithPhoneNumberExceeding25Chars_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var body = new { phoneNumber = new string('1', 26), phoneNumberTypeId = TestConstants.SeededPhoneNumberTypeId };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostPhone_Authenticated_WithBadPhoneNumberTypeId_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var body = new { phoneNumber = "555-000-0001", phoneNumberTypeId = 9999 };

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutPhone_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();
        var body = new { phoneNumber = "555-000-9999" };

        var response = await client.PutAsJsonAsync($"{BaseUrl}/9999", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutPhone_Authenticated_ForNonExistentPhoneType_Returns404()
    {
        var client = CreateAuthenticatedClient();
        var body = new { phoneNumber = "555-000-9999" };

        var response = await client.PutAsJsonAsync($"{BaseUrl}/9999", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePhone_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.DeleteAsync($"{BaseUrl}/9999");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePhone_Authenticated_ForNonExistentPhoneType_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.DeleteAsync($"{BaseUrl}/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
