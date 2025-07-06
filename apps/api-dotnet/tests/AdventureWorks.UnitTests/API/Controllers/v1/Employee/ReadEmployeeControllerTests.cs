using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadEmployeeController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadEmployeeController _sut;

    public ReadEmployeeControllerTests()
    {
        _sut = new ReadEmployeeController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadEmployeeController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadEmployeeController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        const int businessEntityId = 100;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeModel
            {
                Id = businessEntityId,
                FirstName = "John",
                LastName = "Doe",
                JobTitle = "Software Engineer",
                NationalIdNumber = "123456789",
                LoginId = "adventure-works\\john.doe",
                BirthDate = new DateTime(1990, 5, 15),
                HireDate = new DateTime(2020, 1, 10),
                MaritalStatus = "M",
                Gender = "M"
            });

        var result = await _sut.GetByIdAsync(businessEntityId);
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var model = objectResult.Value as EmployeeModel;
            model.Should().NotBeNull();
            model!.Id.Should().Be(businessEntityId);
            model.FirstName.Should().Be("John");
            model.LastName.Should().Be("Doe");
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        const int businessEntityId = 999;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((EmployeeModel?)null);

        var result = await _sut.GetByIdAsync(businessEntityId);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the employee.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-999)]
    public async Task GetById_returns_bad_request_for_invalid_id_Async(int businessEntityId)
    {
        var result = await _sut.GetByIdAsync(businessEntityId);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid business entity ID must be specified.");
        }
    }

    [Fact]
    public async Task GetById_calls_mediator_with_correct_query_Async()
    {
        const int businessEntityId = 274;
        ReadEmployeeQuery? capturedQuery = null;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<EmployeeModel?>, CancellationToken>((query, _) => capturedQuery = query as ReadEmployeeQuery)
            .ReturnsAsync(new EmployeeModel
            {
                Id = businessEntityId,
                FirstName = "Test",
                LastName = "User",
                JobTitle = "Tester",
                NationalIdNumber = "123456789",
                LoginId = "test\\user",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                HireDate = DateTime.UtcNow.AddYears(-5),
                MaritalStatus = "S",
                Gender = "M"
            });

        await _sut.GetByIdAsync(businessEntityId);

        using (new AssertionScope())
        {
            capturedQuery.Should().NotBeNull("because mediator should be called with a query");
            capturedQuery!.BusinessEntityId.Should().Be(businessEntityId,
                "because the query should contain the correct BusinessEntityId");

            _mockMediator.Verify(
                x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()),
                Times.Once,
                "because mediator should be called exactly once");
        }
    }
    
    [Fact]
    public async Task GetById_logs_warning_when_invalid_id_provided_Async()
    {
        const int invalidBusinessEntityId = -5;

        await _sut.GetByIdAsync(invalidBusinessEntityId);

        _mockLogger.VerifyLoggingMessageContains(
            "Invalid business entity ID provided",
            null,
            LogLevel.Warning);
    }

    [Fact]
    public async Task GetById_logs_warning_when_employee_not_found_Async()
    {
        const int businessEntityId = 999;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeModel?)null);

        await _sut.GetByIdAsync(businessEntityId);

        _mockLogger.VerifyLoggingMessageContains(
            "Employee not found with BusinessEntityId",
            null,
            LogLevel.Warning);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(9999)]
    public async Task GetById_handles_various_valid_ids_Async(int businessEntityId)
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeModel
            {
                Id = businessEntityId,
                FirstName = "Test",
                LastName = "Employee",
                JobTitle = "Tester",
                NationalIdNumber = "123456789",
                LoginId = $"test\\employee{businessEntityId}",
                BirthDate = DateTime.UtcNow.AddYears(-25),
                HireDate = DateTime.UtcNow.AddYears(-3),
                MaritalStatus = "S",
                Gender = "M"
            });

        var result = await _sut.GetByIdAsync(businessEntityId);
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var model = objectResult.Value as EmployeeModel;
            model.Should().NotBeNull();
            model!.Id.Should().Be(businessEntityId);
        }
    }

    #region List Endpoint Tests

    [Fact]
    public async Task GetEmployeeList_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeSearchResultModel
            {
                Results = new List<EmployeeModel>
                {
                    new()
                    {
                        Id = 100,
                        FirstName = "John",
                        LastName = "Doe",
                        JobTitle = "Engineer",
                        NationalIdNumber = "123456789",
                        LoginId = "test\\john",
                        BirthDate = DateTime.UtcNow.AddYears(-30),
                        HireDate = DateTime.UtcNow.AddYears(-5),
                        MaritalStatus = "M",
                        Gender = "M"
                    }
                },
                TotalRecords = 1
            });

        var result = await _sut.GetEmployeeListAsync(new EmployeeParameter());
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var searchResult = objectResult.Value as EmployeeSearchResultModel;
            searchResult.Should().NotBeNull();
            searchResult!.Results.Should().HaveCount(1);
            searchResult.TotalRecords.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetEmployeeList_null_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeSearchResultModel { Results = null! });

        var result = await _sut.GetEmployeeListAsync(new EmployeeParameter());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon input query parameters.");

            _mockLogger.VerifyLoggingMessageContains(
                "Unable to locate results based upon input query parameters",
                null,
                LogLevel.Error);
        }
    }

    [Fact]
    public async Task GetEmployeeList_empty_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeSearchResultModel { Results = new List<EmployeeModel>() });

        var result = await _sut.GetEmployeeListAsync(new EmployeeParameter());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon input query parameters.");

            _mockLogger.VerifyLoggingMessageContains(
                "Unable to locate results based upon input query parameters",
                null,
                LogLevel.Error);
        }
    }

    [Fact]
    public async Task GetEmployeeList_calls_mediator_with_correct_parameters_Async()
    {
        ReadEmployeeListQuery? capturedQuery = null;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<EmployeeSearchResultModel>, CancellationToken>((query, _) =>
                capturedQuery = query as ReadEmployeeListQuery)
            .ReturnsAsync(new EmployeeSearchResultModel
            {
                Results = new List<EmployeeModel> { new() { Id = 1, FirstName = "Test", LastName = "User", JobTitle = "Tester", NationalIdNumber = "123", LoginId = "test\\user", BirthDate = DateTime.UtcNow, HireDate = DateTime.UtcNow, MaritalStatus = "S", Gender = "M" } }
            });

        var parameters = new EmployeeParameter { PageNumber = 2, PageSize = 25, OrderBy = "lastName" };
        await _sut.GetEmployeeListAsync(parameters);

        using (new AssertionScope())
        {
            capturedQuery.Should().NotBeNull();
            capturedQuery!.Parameters.Should().NotBeNull();
            capturedQuery.Parameters.PageNumber.Should().Be(2);
            capturedQuery.Parameters.PageSize.Should().Be(25);
            capturedQuery.SearchModel.Should().BeNull();

            _mockMediator.Verify(
                x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()),
                Times.Once,
                "because mediator should be called exactly once");
        }
    }

    #endregion

    #region Search Endpoint Tests

    [Fact]
    public async Task SearchEmployees_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeSearchResultModel
            {
                Results = new List<EmployeeModel>
                {
                    new()
                    {
                        Id = 100,
                        FirstName = "Jane",
                        LastName = "Smith",
                        JobTitle = "Manager",
                        NationalIdNumber = "987654321",
                        LoginId = "test\\jane",
                        BirthDate = DateTime.UtcNow.AddYears(-35),
                        HireDate = DateTime.UtcNow.AddYears(-10),
                        MaritalStatus = "M",
                        Gender = "F"
                    }
                },
                TotalRecords = 1
            });

        var result = await _sut.SearchEmployeesAsync(new EmployeeParameter(), new EmployeeSearchModel());
        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var searchResult = objectResult.Value as EmployeeSearchResultModel;
            searchResult.Should().NotBeNull();
            searchResult!.Results.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task SearchEmployees_null_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeSearchResultModel { Results = null! });

        var result = await _sut.SearchEmployeesAsync(new EmployeeParameter(), new EmployeeSearchModel());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon client input parameters.");

            _mockLogger.VerifyLoggingMessageContains(
                "Unable to locate results based upon client input parameters",
                null,
                LogLevel.Error);
        }
    }

    [Fact]
    public async Task SearchEmployees_empty_results_bad_request_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeSearchResultModel { Results = new List<EmployeeModel>() });

        var result = await _sut.SearchEmployeesAsync(new EmployeeParameter(), new EmployeeSearchModel());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate results based upon client input parameters.");

            _mockLogger.VerifyLoggingMessageContains(
                "Unable to locate results based upon client input parameters",
                null,
                LogLevel.Error);
        }
    }

    [Fact]
    public async Task SearchEmployees_calls_mediator_with_correct_parameters_Async()
    {
        ReadEmployeeListQuery? capturedQuery = null;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<EmployeeSearchResultModel>, CancellationToken>((query, _) =>
                capturedQuery = query as ReadEmployeeListQuery)
            .ReturnsAsync(new EmployeeSearchResultModel
            {
                Results = new List<EmployeeModel> { new() { Id = 1, FirstName = "Test", LastName = "User", JobTitle = "Tester", NationalIdNumber = "123", LoginId = "test\\user", BirthDate = DateTime.UtcNow, HireDate = DateTime.UtcNow, MaritalStatus = "S", Gender = "M" } }
            });

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { FirstName = "John", JobTitle = "Engineer" };

        await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            capturedQuery.Should().NotBeNull();
            capturedQuery!.Parameters.Should().NotBeNull();
            capturedQuery.SearchModel.Should().NotBeNull();
            capturedQuery.SearchModel!.FirstName.Should().Be("John");
            capturedQuery.SearchModel.JobTitle.Should().Be("Engineer");

            _mockMediator.Verify(
                x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()),
                Times.Once,
                "because mediator should be called exactly once");
        }
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 25)]
    [InlineData(5, 50)]
    public async Task SearchEmployees_handles_various_pagination_parameters_Async(int pageNumber, int pageSize)
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeSearchResultModel
            {
                Results = new List<EmployeeModel> { new() { Id = 1, FirstName = "Test", LastName = "User", JobTitle = "Tester", NationalIdNumber = "123", LoginId = "test\\user", BirthDate = DateTime.UtcNow, HireDate = DateTime.UtcNow, MaritalStatus = "S", Gender = "M" } },
                PageNumber = pageNumber,
                PageSize = pageSize
            });

        var parameters = new EmployeeParameter { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _sut.SearchEmployeesAsync(parameters, new EmployeeSearchModel());

        var objectResult = result as ObjectResult;
        var searchResult = objectResult!.Value as EmployeeSearchResultModel;

        using (new AssertionScope())
        {
            searchResult.Should().NotBeNull();
            searchResult!.PageNumber.Should().Be(pageNumber);
            searchResult.PageSize.Should().Be(pageSize);
        }
    }

    #endregion
}
