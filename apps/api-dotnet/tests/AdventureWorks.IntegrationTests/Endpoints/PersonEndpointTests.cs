using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Person;
using FluentAssertions.Execution;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

public sealed class PersonEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static readonly string BaseUrl = $"/api/v1.0/persons/{TestConstants.SeededPersonId}";
    private const string NonExistentPersonUrl = "/api/v1.0/persons/999999";
    private const string InvalidPersonUrl = "/api/v1.0/persons/-1";

    [Fact]
    public async Task GetPerson_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPerson_Authenticated_ForSeededPerson_Returns200()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var person = await DeserializeAsync<PersonDetailModel>(response);
        using (new AssertionScope())
        {
            person.Should().NotBeNull();
            person!.BusinessEntityId.Should().Be(TestConstants.SeededPersonId);
            person.FirstName.Should().NotBeNullOrWhiteSpace();
            person.LastName.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetPerson_Authenticated_ForNonExistentPerson_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(NonExistentPersonUrl);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPerson_Authenticated_ForInvalidPersonId_Returns400()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(InvalidPersonUrl);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

}
