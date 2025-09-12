using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.Features.Person.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

/// <summary>
/// Unit tests for SearchPersonsQueryHandler.
/// Tests validation, filtering, pagination, and edge cases for person search functionality.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SearchPersonsQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonRepository> _mockPersonRepository = new();
    private readonly IValidator<SearchPersonsQuery> _validator = new SearchPersonsQueryValidator();
    private readonly SearchPersonsQueryHandler _sut;

    public SearchPersonsQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(PersonEntityToSearchPersonsModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new SearchPersonsQueryHandler(_mapper, _mockPersonRepository.Object, _validator);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            ((Action)(() => _ = new SearchPersonsQueryHandler(null!, _mockPersonRepository.Object, _validator)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            ((Action)(() => _ = new SearchPersonsQueryHandler(_mapper, null!, _validator)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personRepository");

            ((Action)(() => _ = new SearchPersonsQueryHandler(_mapper, _mockPersonRepository.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    /// <summary>
    /// Test: Valid single filter (firstName)
    /// Handler receives query with firstName="John", lastName=null, personTypeCode=null, page=1, pageSize=10
    /// Mock repo returns 5 persons with "John"
    /// Assert: 5 results returned, totalCount=5, pageNumber=1, pageSize=10
    /// </summary>
    [Fact]
    public async Task Handle_returns_filtered_results_by_firstName_only()
    {
        var mockPersons = GetMockPersonsWithFirstName("John", 5).ToList();

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "John",
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 5));

        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            LastName = null,
            PersonTypeCode = null,
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);
            result.TotalCount.Should().Be(5);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.Items.Should().AllSatisfy(p => p.FirstName.Should().Contain("John"));
        }
    }

    /// <summary>
    /// Test: Valid multiple filters (AND logic)
    /// Handler receives firstName="John", lastName="Doe", personTypeCode="EM", page=1, pageSize=10
    /// Mock repo returns 2 persons matching all three filters
    /// Assert: 2 results, correct filter combination
    /// </summary>
    [Fact]
    public async Task Handle_returns_filtered_results_with_multiple_filters()
    {
        var mockPersons = new List<PersonEntity>
        {
            CreatePersonEntity(1, "John", "Doe", "EM", "Employee"),
            CreatePersonEntity(2, "John", "Doe", "EM", "Employee")
        };

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "John",
                "Doe",
                "EM",
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 2));

        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            LastName = "Doe",
            PersonTypeCode = "EM",
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.Should().AllSatisfy(p =>
            {
                p.FirstName.Should().Be("John");
                p.LastName.Should().Be("Doe");
                p.PersonTypeName.Should().Be("Employee");
            });
        }
    }

    /// <summary>
    /// Test: No filters provided (validation failure)
    /// Handler receives firstName=null, lastName=null, personTypeCode=null
    /// Should throw ValidationException or ArgumentException
    /// Assert: Exception message contains "At least one search criterion"
    /// </summary>
    [Fact]
    public async Task Handle_throws_ArgumentException_when_no_filters_provided()
    {
        var query = new SearchPersonsQuery
        {
            FirstName = null,
            LastName = null,
            PersonTypeCode = null,
            Page = 1,
            PageSize = 10
        };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<ArgumentException>();

        exception.And.Message.Should().Contain("At least one search criterion");
    }

    /// <summary>
    /// Test: No filters provided with empty strings (validation failure)
    /// Handler receives firstName="", lastName="", personTypeCode=""
    /// Should throw ArgumentException
    /// Assert: Exception message contains "At least one search criterion"
    /// </summary>
    [Fact]
    public async Task Handle_throws_ArgumentException_when_all_filters_empty_strings()
    {
        var query = new SearchPersonsQuery
        {
            FirstName = "   ",
            LastName = "",
            PersonTypeCode = string.Empty,
            Page = 1,
            PageSize = 10
        };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<ArgumentException>();

        exception.And.Message.Should().Contain("At least one search criterion");
    }

    /// <summary>
    /// Test: Invalid pageSize (> 100)
    /// Handler receives pageSize=101
    /// Should throw ArgumentException
    /// Assert: Exception message contains "pageSize" and "100"
    /// </summary>
    [Fact]
    public async Task Handle_throws_ArgumentException_when_pageSize_exceeds_100()
    {
        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            PageSize = 101
        };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<ArgumentException>();

        exception.And.Message.ToLower().Should().Contain("pagesize");
        exception.And.Message.Should().Contain("100");
    }

    /// <summary>
    /// Test: Invalid pageSize (< 1)
    /// Handler receives pageSize=0
    /// Should throw ArgumentException
    /// </summary>
    [Fact]
    public async Task Handle_throws_ArgumentException_when_pageSize_is_zero()
    {
        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            PageSize = 0
        };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<ArgumentException>();

        exception.And.Message.ToLower().Should().Contain("pagesize");
    }

    /// <summary>
    /// Test: Invalid page (< 1)
    /// Handler receives page=0
    /// Should throw ArgumentException
    /// Assert: Exception message contains "page" or "Page"
    /// </summary>
    [Fact]
    public async Task Handle_throws_ArgumentException_when_page_is_zero()
    {
        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            Page = 0
        };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<ArgumentException>();

        exception.And.Message.ToLower().Should().Contain("page");
    }

    /// <summary>
    /// Test: Invalid page (< 1)
    /// Handler receives page=-5
    /// Should throw ArgumentException
    /// </summary>
    [Fact]
    public async Task Handle_throws_ArgumentException_when_page_is_negative()
    {
        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            Page = -5
        };

        var act = async () => await _sut.Handle(query, CancellationToken.None);

        var exception = await act.Should()
            .ThrowAsync<ArgumentException>();

        exception.And.Message.ToLower().Should().Contain("page");
    }

    /// <summary>
    /// Test: Pagination metadata
    /// Mock repo: 250 total results, page=2, pageSize=20
    /// Assert: pageNumber=2, pageSize=20, totalCount=250
    /// Verify pagination calculation: skip=20, take=20 (handled by repo)
    /// </summary>
    [Fact]
    public async Task Handle_returns_correct_pagination_metadata()
    {
        var mockPersons = GetMockPersonsWithFirstName("S", 20).ToList();

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "S",
                null,
                null,
                2,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 250));

        var query = new SearchPersonsQuery
        {
            FirstName = "S",
            Page = 2,
            PageSize = 20
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(20);
            result.TotalCount.Should().Be(250);
            result.Items.Should().HaveCount(20);
        }
    }

    /// <summary>
    /// Test: Empty results (no matches)
    /// Mock repo returns empty list, totalCount=0
    /// Assert: results.Items is empty, totalCount=0
    /// </summary>
    [Fact]
    public async Task Handle_returns_empty_results_when_no_matches()
    {
        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "ZZZZZZZZ",
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PersonEntity>(), 0));

        var query = new SearchPersonsQuery
        {
            FirstName = "ZZZZZZZZ",
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }

    /// <summary>
    /// Test: Null PersonType handling
    /// Mock person with personTypeId but PersonType=null
    /// Assert: personTypeName is empty string (not null)
    /// </summary>
    [Fact]
    public async Task Handle_returns_empty_string_for_null_PersonType()
    {
        var mockPerson = new PersonEntity
        {
            BusinessEntityId = 1,
            PersonTypeId = 1,
            FirstName = "John",
            LastName = "Doe",
            PersonType = null,
            EmailAddresses = new List<EmailAddressEntity>(),
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "John",
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PersonEntity> { mockPerson }, 1));

        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(1);
            result.Items[0].PersonTypeName.Should().Be(string.Empty);
            result.Items[0].PersonTypeName.Should().NotBeNull();
        }
    }

    /// <summary>
    /// Test: Null/Missing email
    /// Mock person with no email addresses
    /// Assert: primaryEmail is null (not throw)
    /// </summary>
    [Fact]
    public async Task Handle_returns_null_email_when_no_email_addresses()
    {
        var mockPerson = new PersonEntity
        {
            BusinessEntityId = 1,
            PersonTypeId = 1,
            FirstName = "John",
            LastName = "Doe",
            PersonType = new PersonTypeEntity
            {
                PersonTypeId = 1,
                PersonTypeCode = "EM",
                PersonTypeName = "Employee",
                PersonTypeDescription = "Test"
            },
            EmailAddresses = new List<EmailAddressEntity>(),
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "John",
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PersonEntity> { mockPerson }, 1));

        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(1);
            result.Items[0].PrimaryEmail.Should().BeNull();
        }
    }

    /// <summary>
    /// Test: Search with LastName filter only
    /// Handler receives lastName="Doe", firstName=null, personTypeCode=null
    /// Mock repo returns persons with "Doe" lastname
    /// </summary>
    [Fact]
    public async Task Handle_returns_filtered_results_by_lastName_only()
    {
        var mockPersons = GetMockPersonsWithLastName("Doe", 3).ToList();

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                null,
                "Doe",
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 3));

        var query = new SearchPersonsQuery
        {
            FirstName = null,
            LastName = "Doe",
            PersonTypeCode = null,
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(3);
            result.TotalCount.Should().Be(3);
            result.Items.Should().AllSatisfy(p => p.LastName.Should().Contain("Doe"));
        }
    }

    /// <summary>
    /// Test: Search with PersonTypeCode filter only
    /// Handler receives personTypeCode="EM", firstName=null, lastName=null
    /// Mock repo returns persons with personTypeCode="EM"
    /// </summary>
    [Fact]
    public async Task Handle_returns_filtered_results_by_personTypeCode_only()
    {
        var mockPersons = GetMockPersonsByType("EM", "Employee", 4).ToList();

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                null,
                null,
                "EM",
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 4));

        var query = new SearchPersonsQuery
        {
            FirstName = null,
            LastName = null,
            PersonTypeCode = "EM",
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(4);
            result.TotalCount.Should().Be(4);
            result.Items.Should().AllSatisfy(p => p.PersonTypeName.Should().Be("Employee"));
        }
    }

    /// <summary>
    /// Test: PersonEntity with email address
    /// Mock person with email address
    /// Assert: primaryEmail is populated
    /// </summary>
    [Fact]
    public async Task Handle_returns_primary_email_when_email_exists()
    {
        var mockPerson = new PersonEntity
        {
            BusinessEntityId = 1,
            PersonTypeId = 1,
            FirstName = "John",
            LastName = "Doe",
            PersonType = new PersonTypeEntity
            {
                PersonTypeId = 1,
                PersonTypeCode = "EM",
                PersonTypeName = "Employee",
                PersonTypeDescription = "Test"
            },
            EmailAddresses = new List<EmailAddressEntity>
            {
                new EmailAddressEntity
                {
                    BusinessEntityId = 1,
                    EmailAddressId = 11,
                    EmailAddressName = "john.doe@example.com",
                    ModifiedDate = DefaultAuditDate
                }
            },
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "John",
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PersonEntity> { mockPerson }, 1));

        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            Page = 1,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(1);
            result.Items[0].PrimaryEmail.Should().Be("john.doe@example.com");
        }
    }

    /// <summary>
    /// Test: Page size boundary test (pageSize=100 is valid)
    /// Handler receives pageSize=100
    /// Should not throw exception
    /// </summary>
    [Fact]
    public async Task Handle_accepts_pageSize_of_100()
    {
        var mockPersons = GetMockPersonsWithFirstName("John", 100).ToList();

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "John",
                null,
                null,
                1,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 100));

        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            Page = 1,
            PageSize = 100
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(100);
            result.PageSize.Should().Be(100);
        }
    }

    /// <summary>
    /// Test: Page size boundary test (pageSize=1 is valid)
    /// Handler receives pageSize=1
    /// Should not throw exception
    /// </summary>
    [Fact]
    public async Task Handle_accepts_pageSize_of_1()
    {
        var mockPersons = GetMockPersonsWithFirstName("John", 1).ToList();

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "John",
                null,
                null,
                1,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 150));

        var query = new SearchPersonsQuery
        {
            FirstName = "John",
            Page = 1,
            PageSize = 1
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(1);
            result.PageSize.Should().Be(1);
            result.TotalCount.Should().Be(150);
        }
    }

    /// <summary>
    /// Test: Last page with fewer results
    /// Mock repo: 25 total results, page=3, pageSize=10 (3rd page has 5 results)
    /// Assert: results contain 5 items, totalCount=25
    /// </summary>
    [Fact]
    public async Task Handle_returns_correct_results_for_last_partial_page()
    {
        var mockPersons = GetMockPersonsWithFirstName("S", 5).ToList();

        _mockPersonRepository.Setup(x =>
            x.SearchAsync(
                "S",
                null,
                null,
                3,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockPersons, 25));

        var query = new SearchPersonsQuery
        {
            FirstName = "S",
            Page = 3,
            PageSize = 10
        };

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Items.Should().HaveCount(5);
            result.TotalCount.Should().Be(25);
            result.PageNumber.Should().Be(3);
        }
    }

    // ===============================
    // Helper Methods
    // ===============================

    private static IEnumerable<PersonEntity> GetMockPersonsWithFirstName(string firstName, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return CreatePersonEntity(
                1000 + i,
                firstName,
                $"LastName{i}",
                "EM",
                "Employee");
        }
    }

    private static IEnumerable<PersonEntity> GetMockPersonsWithLastName(string lastName, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return CreatePersonEntity(
                1000 + i,
                $"FirstName{i}",
                lastName,
                "EM",
                "Employee");
        }
    }

    private static IEnumerable<PersonEntity> GetMockPersonsByType(string typeCode, string typeName, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return CreatePersonEntity(
                1000 + i,
                $"FirstName{i}",
                $"LastName{i}",
                typeCode,
                typeName);
        }
    }

    private static PersonEntity CreatePersonEntity(
        int id,
        string firstName,
        string lastName,
        string typeCode,
        string typeName)
    {
        return new PersonEntity
        {
            BusinessEntityId = id,
            PersonTypeId = 1,
            FirstName = firstName,
            LastName = lastName,
            PersonType = new PersonTypeEntity
            {
                PersonTypeId = 1,
                PersonTypeCode = typeCode,
                PersonTypeName = typeName,
                PersonTypeDescription = "Test description"
            },
            EmailAddresses = new List<EmailAddressEntity>
            {
                new EmailAddressEntity
                {
                    BusinessEntityId = id,
                    EmailAddressId = 10 + id,
                    EmailAddressName = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                    ModifiedDate = DefaultAuditDate
                }
            },
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };
    }
}
