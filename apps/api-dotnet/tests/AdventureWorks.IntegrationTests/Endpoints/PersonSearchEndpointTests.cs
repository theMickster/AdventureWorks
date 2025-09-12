using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.Person;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for the Search Persons endpoint (GET /api/v1.0/persons).
/// Tests authentication, validation, filtering, pagination, and edge cases.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PersonSearchEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string BaseUrl = "/api/v1.0/persons";

    // ===============================
    // Authentication Tests
    // ===============================

    [Fact]
    public async Task SearchPersons_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Ken");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ===============================
    // Validation Tests
    // ===============================

    [Fact]
    public async Task SearchPersons_NoFilters_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("At least one search criterion");
    }

    [Fact]
    public async Task SearchPersons_PageSizeExceeds100_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Ken&pageSize=101");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ToLower().Should().Contain("pagesize");
    }

    [Fact]
    public async Task SearchPersons_PageIsZero_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Ken&page=0");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchPersons_PageIsNegative_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Ken&page=-1");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchPersons_PageSizeIsZero_Returns400()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Ken&pageSize=0");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ===============================
    // Search Functionality Tests
    // ===============================

    [Fact]
    public async Task SearchPersons_ByFirstName_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Integration&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().BeGreaterThan(0);
            result.Items.Should().AllSatisfy(p =>
                p.FirstName.Should().ContainEquivalentOf("Integration"));
        }
    }

    [Fact]
    public async Task SearchPersons_ByPersonTypeCode_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?personTypeCode=EM&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
            result.TotalCount.Should().BeGreaterThan(0);
            result.Items.Should().AllSatisfy(p => p.PersonTypeName.Should().Be("Employee"));
        }
    }

    [Fact]
    public async Task SearchPersons_WithMultipleFilters_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Integration&personTypeCode=EM&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
            result.Items.Should().AllSatisfy(p =>
            {
                p.FirstName.Should().ContainEquivalentOf("Integration");
                p.PersonTypeName.Should().Be("Employee");
            });
        }
    }

    [Fact]
    public async Task SearchPersons_FirstPage_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Integration&page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeEmpty();
            result.Items.Count.Should().BeLessThanOrEqualTo(5);
            result.PageNumber.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchPersons_NoMatches_Returns200WithEmptyResults()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=ZZZZZZZZZZZZZZZZZZZZ&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task SearchPersons_CaseInsensitive_Returns200()
    {
        var client = CreateAuthenticatedClient();

        var responseLower = await client.GetAsync($"{BaseUrl}?firstName=integration&page=1&pageSize=10");
        var resultLower = await DeserializeAsync<SearchPersonsQueryResult>(responseLower);

        var responseMixed = await client.GetAsync($"{BaseUrl}?firstName=Integration&page=1&pageSize=10");
        var resultMixed = await DeserializeAsync<SearchPersonsQueryResult>(responseMixed);

        resultLower?.TotalCount.Should().Be(resultMixed?.TotalCount);
    }

    [Fact]
    public async Task SearchPersons_ByLastName_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?lastName=TestPerson&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }
    }

    [Fact]
    public async Task SearchPersons_ResponseStructure_ContainsAllRequiredFields()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Integration&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
            result.Items.Should().AllSatisfy(p =>
            {
                p.BusinessEntityId.Should().BeGreaterThan(0);
                p.FirstName.Should().NotBeNullOrWhiteSpace();
                p.LastName.Should().NotBeNullOrWhiteSpace();
                p.PersonTypeName.Should().NotBeNull();
            });
        }
    }

    [Fact]
    public async Task SearchPersons_WithSeededData_Returns200()
    {
        await SeedAsync(async context =>
        {
            var personType = context.PersonTypes.FirstOrDefault(pt => pt.PersonTypeCode == "EM");
            if (personType == null)
            {
                personType = new PersonTypeEntity
                {
                    PersonTypeId = 99,
                    PersonTypeCode = "EM",
                    PersonTypeName = "Employee",
                    PersonTypeDescription = "Employee"
                };
                context.PersonTypes.Add(personType);
            }

            context.BusinessEntities.Add(new BusinessEntity
            {
                BusinessEntityId = 9999,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            });

            context.Persons.Add(new PersonEntity
            {
                BusinessEntityId = 9999,
                PersonTypeId = personType.PersonTypeId,
                FirstName = "SearchTest",
                LastName = "Person",
                NameStyle = false,
                EmailPromotion = 0,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow,
                PersonType = personType
            });

            await Task.CompletedTask;
        });

        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=SearchTest&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().Contain(p => p.FirstName == "SearchTest" && p.LastName == "Person");
        }
    }

    [Fact]
    public async Task SearchPersons_MultiplePersonTypes_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?personTypeCode=EM&page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
            result.Items.Should().AllSatisfy(p => p.PersonTypeName.Should().Be("Employee"));
        }
    }

    [Fact]
    public async Task SearchPersons_PartialNameMatch_Returns200()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{BaseUrl}?firstName=Test&page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await DeserializeAsync<SearchPersonsQueryResult>(response);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Items.Should().NotBeNull();
        }
    }
}
