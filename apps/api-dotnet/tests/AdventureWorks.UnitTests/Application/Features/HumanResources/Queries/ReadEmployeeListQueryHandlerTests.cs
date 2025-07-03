using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private ReadEmployeeListQueryHandler _sut;

    public ReadEmployeeListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeeEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEmployeeListQueryHandler(_mapper, _mockEmployeeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadEmployeeListQueryHandler(
                    null!,
                    _mockEmployeeRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadEmployeeListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("employeeRepository");
        }
    }

    [Fact]
    public async Task Handle_list_returns_correct_null_result_Async()
    {
        _mockEmployeeRepository.Setup(x => x.GetEmployeesAsync(It.IsAny<EmployeeParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null!, 0));

        var result = await _sut.Handle(
            new ReadEmployeeListQuery { Parameters = new EmployeeParameter() },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_list_returns_correct_empty_result_Async()
    {
        var readOnlyList = new List<EmployeeEntity>().AsReadOnly();
        _mockEmployeeRepository.Setup(x => x.GetEmployeesAsync(It.IsAny<EmployeeParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((readOnlyList, 0));

        var result = await _sut.Handle(
            new ReadEmployeeListQuery { Parameters = new EmployeeParameter() },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_list_returns_valid_paged_model_Async()
    {
        var mockEmployees = GetMockEmployeeEntities();
        _mockEmployeeRepository.Setup(x => x.GetEmployeesAsync(It.IsAny<EmployeeParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockEmployees, 3));

        var param = new EmployeeParameter
        {
            PageNumber = 1,
            OrderBy = "firstName",
            PageSize = 30,
            SortOrder = "DESCENDING"
        };

        var pagedResult = await _sut.Handle(
            new ReadEmployeeListQuery { Parameters = param },
            CancellationToken.None);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();
            pagedResult.PageNumber.Should().Be(1);
            pagedResult.PageSize.Should().Be(30);
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse();
            pagedResult.TotalPages.Should().Be(1);
            pagedResult.TotalRecords.Should().Be(3);
            pagedResult.Results.Should().HaveCount(3);

            var employee01 = pagedResult.Results.FirstOrDefault(x => x.Id == 100);
            var employee02 = pagedResult.Results.FirstOrDefault(x => x.Id == 101);

            employee01.Should().NotBeNull();
            employee01!.FirstName.Should().Be("John");
            employee01!.LastName.Should().Be("Doe");
            employee01!.JobTitle.Should().Be("Software Engineer");

            employee02.Should().NotBeNull();
            employee02!.FirstName.Should().Be("Jane");
            employee02!.LastName.Should().Be("Smith");
            employee02!.JobTitle.Should().Be("Senior Developer");
        }
    }

    [Fact]
    public async Task Handle_search_returns_correct_null_result_Async()
    {
        _mockEmployeeRepository.Setup(x => x.SearchEmployeesAsync(
                It.IsAny<EmployeeParameter>(),
                It.IsAny<EmployeeSearchModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((null!, 0));

        var result = await _sut.Handle(
            new ReadEmployeeListQuery
            {
                Parameters = new EmployeeParameter(),
                SearchModel = new EmployeeSearchModel()
            },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_search_returns_correct_empty_result_Async()
    {
        var readOnlyList = new List<EmployeeEntity>().AsReadOnly();
        _mockEmployeeRepository.Setup(x => x.SearchEmployeesAsync(
                It.IsAny<EmployeeParameter>(),
                It.IsAny<EmployeeSearchModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((readOnlyList, 0));

        var result = await _sut.Handle(
            new ReadEmployeeListQuery
            {
                Parameters = new EmployeeParameter(),
                SearchModel = new EmployeeSearchModel()
            },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_search_returns_valid_paged_model_Async()
    {
        var mockEmployees = GetMockEmployeeEntities();
        _mockEmployeeRepository.Setup(x => x.SearchEmployeesAsync(
                It.IsAny<EmployeeParameter>(),
                It.IsAny<EmployeeSearchModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((mockEmployees.Where(x => x.BusinessEntityId == 100).ToList(), 1));

        var queryParam = new EmployeeParameter
        {
            PageNumber = 1,
            OrderBy = "lastName",
            PageSize = 10,
            SortOrder = "ASC"
        };
        var searchParam = new EmployeeSearchModel { Id = 100 };

        var pagedResult = await _sut.Handle(
            new ReadEmployeeListQuery { Parameters = queryParam, SearchModel = searchParam },
            CancellationToken.None);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();
            pagedResult.PageNumber.Should().Be(1);
            pagedResult.PageSize.Should().Be(10);
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse();
            pagedResult.TotalPages.Should().Be(1);
            pagedResult.TotalRecords.Should().Be(1);
            pagedResult.Results.Should().HaveCount(1);

            var employee01 = pagedResult.Results.FirstOrDefault(x => x.Id == 100);
            employee01.Should().NotBeNull();
            employee01!.FirstName.Should().Be("John");
            employee01!.LastName.Should().Be("Doe");
            employee01!.JobTitle.Should().Be("Software Engineer");
        }
    }

    [Fact]
    public async Task Handle_search_by_job_title_returns_filtered_results_Async()
    {
        var mockEmployees = GetMockEmployeeEntities();
        var filteredEmployees = mockEmployees.Where(x => x.JobTitle.Contains("Engineer")).ToList();

        _mockEmployeeRepository.Setup(x => x.SearchEmployeesAsync(
                It.IsAny<EmployeeParameter>(),
                It.IsAny<EmployeeSearchModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((filteredEmployees, 2));

        var queryParam = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchParam = new EmployeeSearchModel { JobTitle = "Engineer" };

        var pagedResult = await _sut.Handle(
            new ReadEmployeeListQuery { Parameters = queryParam, SearchModel = searchParam },
            CancellationToken.None);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();
            pagedResult.Results.Should().HaveCount(2);
            pagedResult.TotalRecords.Should().Be(2);
            pagedResult.Results.Should().OnlyContain(e => e.JobTitle.Contains("Engineer"));
        }
    }

    [Fact]
    public async Task Handle_calls_repository_with_correct_parameters_for_list_Async()
    {
        var mockEmployees = GetMockEmployeeEntities();
        var capturedParameter = (EmployeeParameter?)null;

        _mockEmployeeRepository.Setup(x => x.GetEmployeesAsync(
                It.IsAny<EmployeeParameter>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeParameter, CancellationToken>((param, _) => capturedParameter = param)
            .ReturnsAsync((mockEmployees, 3));

        var param = new EmployeeParameter { PageNumber = 2, PageSize = 15, OrderBy = "lastName" };

        await _sut.Handle(
            new ReadEmployeeListQuery { Parameters = param },
            CancellationToken.None);

        using (new AssertionScope())
        {
            capturedParameter.Should().NotBeNull();
            capturedParameter!.PageNumber.Should().Be(2);
            capturedParameter.PageSize.Should().Be(15);
            capturedParameter.OrderBy.Should().NotBeNullOrEmpty();

            _mockEmployeeRepository.Verify(
                x => x.GetEmployeesAsync(It.IsAny<EmployeeParameter>(), It.IsAny<CancellationToken>()),
                Times.Once,
                "because the handler should call GetEmployeesAsync exactly once");
        }
    }

    [Fact]
    public async Task Handle_calls_repository_with_correct_parameters_for_search_Async()
    {
        var mockEmployees = GetMockEmployeeEntities();
        var capturedParameter = (EmployeeParameter?)null;
        var capturedSearchModel = (EmployeeSearchModel?)null;

        _mockEmployeeRepository.Setup(x => x.SearchEmployeesAsync(
                It.IsAny<EmployeeParameter>(),
                It.IsAny<EmployeeSearchModel>(),
                It.IsAny<CancellationToken>()))
            .Callback<EmployeeParameter, EmployeeSearchModel, CancellationToken>(
                (param, search, _) =>
                {
                    capturedParameter = param;
                    capturedSearchModel = search;
                })
            .ReturnsAsync((mockEmployees.Take(1).ToList(), 1));

        var param = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { FirstName = "John", JobTitle = "Engineer" };

        await _sut.Handle(
            new ReadEmployeeListQuery { Parameters = param, SearchModel = searchModel },
            CancellationToken.None);

        using (new AssertionScope())
        {
            capturedParameter.Should().NotBeNull();
            capturedSearchModel.Should().NotBeNull();
            capturedSearchModel!.FirstName.Should().Be("John");
            capturedSearchModel.JobTitle.Should().Be("Engineer");

            _mockEmployeeRepository.Verify(
                x => x.SearchEmployeesAsync(
                    It.IsAny<EmployeeParameter>(),
                    It.IsAny<EmployeeSearchModel>(),
                    It.IsAny<CancellationToken>()),
                Times.Once,
                "because the handler should call SearchEmployeesAsync exactly once");
        }
    }

    #region Private Helper Methods

    private static List<EmployeeEntity> GetMockEmployeeEntities()
    {
        var modifiedDate = new DateTime(2023, 1, 1);

        return new List<EmployeeEntity>
        {
            new()
            {
                BusinessEntityId = 100,
                NationalIdnumber = "123456789",
                LoginId = "adventure-works\\john.doe",
                JobTitle = "Software Engineer",
                BirthDate = new DateTime(1990, 5, 15),
                HireDate = new DateTime(2020, 1, 10),
                MaritalStatus = "M",
                Gender = "M",
                SalariedFlag = true,
                OrganizationLevel = 2,
                CurrentFlag = true,
                VacationHours = 40,
                SickLeaveHours = 20,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate,
                PersonBusinessEntity = new PersonEntity
                {
                    BusinessEntityId = 100,
                    FirstName = "John",
                    LastName = "Doe",
                    MiddleName = "Michael",
                    ModifiedDate = modifiedDate,
                    EmailAddresses = new List<EmailAddressEntity>
                    {
                        new()
                        {
                            BusinessEntityId = 100,
                            EmailAddressId = 1,
                            EmailAddressName = "john.doe@adventure-works.com",
                            Rowguid = Guid.NewGuid(),
                            ModifiedDate = modifiedDate
                        }
                    }
                }
            },
            new()
            {
                BusinessEntityId = 101,
                NationalIdnumber = "987654321",
                LoginId = "adventure-works\\jane.smith",
                JobTitle = "Senior Developer",
                BirthDate = new DateTime(1988, 8, 22),
                HireDate = new DateTime(2018, 3, 15),
                MaritalStatus = "S",
                Gender = "F",
                SalariedFlag = true,
                OrganizationLevel = 3,
                CurrentFlag = true,
                VacationHours = 50,
                SickLeaveHours = 15,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate,
                PersonBusinessEntity = new PersonEntity
                {
                    BusinessEntityId = 101,
                    FirstName = "Jane",
                    LastName = "Smith",
                    ModifiedDate = modifiedDate,
                    EmailAddresses = new List<EmailAddressEntity>
                    {
                        new()
                        {
                            BusinessEntityId = 101,
                            EmailAddressId = 1,
                            EmailAddressName = "jane.smith@adventure-works.com",
                            Rowguid = Guid.NewGuid(),
                            ModifiedDate = modifiedDate
                        }
                    }
                }
            },
            new()
            {
                BusinessEntityId = 102,
                NationalIdnumber = "555666777",
                LoginId = "adventure-works\\bob.johnson",
                JobTitle = "QA Engineer",
                BirthDate = new DateTime(1992, 12, 5),
                HireDate = new DateTime(2021, 6, 1),
                MaritalStatus = "M",
                Gender = "M",
                SalariedFlag = false,
                OrganizationLevel = 1,
                CurrentFlag = true,
                VacationHours = 30,
                SickLeaveHours = 10,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate,
                PersonBusinessEntity = new PersonEntity
                {
                    BusinessEntityId = 102,
                    FirstName = "Bob",
                    LastName = "Johnson",
                    ModifiedDate = modifiedDate,
                    EmailAddresses = new List<EmailAddressEntity>
                    {
                        new()
                        {
                            BusinessEntityId = 102,
                            EmailAddressId = 1,
                            EmailAddressName = "bob.johnson@adventure-works.com",
                            Rowguid = Guid.NewGuid(),
                            ModifiedDate = modifiedDate
                        }
                    }
                }
            }
        };
    }

    #endregion
}
