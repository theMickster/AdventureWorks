using AdventureWorks.API.Controllers.v1.Departments;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Departments;

[ExcludeFromCodeCoverage]
public sealed class ReadDepartmentReportingControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadDepartmentReportingController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadDepartmentReportingController _sut;

    public ReadDepartmentReportingControllerTests()
    {
        _sut = new ReadDepartmentReportingController(_mockLogger.Object, _mockMediator.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public void Controller_has_Authorize_attribute()
    {
        typeof(ReadDepartmentReportingController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        ((Action)(() => _ = new ReadDepartmentReportingController(null!, _mockMediator.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_throws_when_mediator_is_null()
    {
        ((Action)(() => _ = new ReadDepartmentReportingController(_mockLogger.Object, null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mediator");
    }

    // GetHeadcountAsync

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    public async Task GetHeadcount_returns_bad_request_for_invalid_departmentIdAsync(short id)
    {
        var result = await _sut.GetHeadcountAsync(id, CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetHeadcount_propagates_KeyNotFoundException_when_department_missingAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadDepartmentHeadcountQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Department with ID 999 not found."));

        Func<Task> act = async () => await _sut.GetHeadcountAsync(999, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Department with ID 999 not found.");
    }

    [Fact]
    public async Task GetHeadcount_returns_ok_with_modelAsync()
    {
        var model = new DepartmentHeadcountModel { DepartmentId = 1, DepartmentName = "Engineering", ActiveEmployeeCount = 5 };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadDepartmentHeadcountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var result = await _sut.GetHeadcountAsync(1, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(model);
    }

    // GetHeadcountSummaryAsync

    [Fact]
    public async Task GetHeadcountSummary_returns_ok_with_listAsync()
    {
        var list = new List<DepartmentHeadcountSummaryModel>
        {
            new() { DepartmentId = 1, DepartmentName = "Engineering", GroupName = "R&D", ActiveEmployeeCount = 10 }
        }.AsReadOnly();

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadDepartmentHeadcountSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _sut.GetHeadcountSummaryAsync(CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    // GetEmployeesAsync

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    public async Task GetEmployees_returns_bad_request_for_invalid_departmentIdAsync(short id)
    {
        var result = await _sut.GetEmployeesAsync(id, 1, 20, CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetEmployees_returns_bad_request_for_invalid_pageAsync(int page)
    {
        var result = await _sut.GetEmployeesAsync(1, page, 20, CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetEmployees_returns_bad_request_for_invalid_pageSizeAsync(int pageSize)
    {
        var result = await _sut.GetEmployeesAsync(1, 1, pageSize, CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetEmployees_returns_bad_request_when_pageSize_exceeds_100Async()
    {
        var result = await _sut.GetEmployeesAsync(1, 1, 101, CancellationToken.None);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetEmployees_propagates_KeyNotFoundException_when_department_missingAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeesByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Department with ID 999 not found."));

        Func<Task> act = async () => await _sut.GetEmployeesAsync(999, 1, 20, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Department with ID 999 not found.");
    }

    [Fact]
    public async Task GetEmployees_returns_ok_and_sets_total_count_headerAsync()
    {
        var employees = new List<EmployeeModel>
        {
            new()
            {
                Id = 1, FirstName = "Alice", LastName = "Anderson", JobTitle = "Engineer",
                MaritalStatus = "S", Gender = "F", NationalIdNumber = "123", LoginId = "alice"
            }
        }.AsReadOnly();

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadEmployeesByDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((employees, 1));

        var result = await _sut.GetEmployeesAsync(1, 1, 20, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeOfType<OkObjectResult>();
            _sut.Response.Headers["X-Total-Count"].ToString().Should().Be("1");
        }
    }
}
