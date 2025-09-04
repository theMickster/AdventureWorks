using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeDepartmentHistoryControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadEmployeeDepartmentHistoryController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadEmployeeDepartmentHistoryController _sut;

    public ReadEmployeeDepartmentHistoryControllerTests()
    {
        _sut = new ReadEmployeeDepartmentHistoryController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        ((Action)(() => _ = new ReadEmployeeDepartmentHistoryController(null!, _mockMediator.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_throws_when_mediator_is_null()
    {
        ((Action)(() => _ = new ReadEmployeeDepartmentHistoryController(_mockLogger.Object, null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public void GetHistoryAsync_has_authorize_attribute_on_controller()
    {
        var attribute = typeof(ReadEmployeeDepartmentHistoryController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .FirstOrDefault() as AuthorizeAttribute;

        attribute.Should().NotBeNull("because the controller must require authentication for all actions");
    }

    [Fact]
    public async Task GetHistoryAsync_returns_bad_request_when_employee_id_is_zeroAsync()
    {
        var result = await _sut.GetHistoryAsync(0, CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task GetHistoryAsync_returns_bad_request_when_employee_id_is_negativeAsync()
    {
        var result = await _sut.GetHistoryAsync(-5, CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task GetHistoryAsync_returns_200_with_resultAsync()
    {
        const int employeeId = 1;

        var history = new List<EmployeeDepartmentHistoryModel>
        {
            new()
            {
                DepartmentId = 2,
                DepartmentName = "Tool Design",
                ShiftId = 1,
                ShiftName = "Day",
                StartDate = new DateTime(2023, 6, 1),
                EndDate = null
            }
        }.AsReadOnly();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeDepartmentHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _sut.GetHistoryAsync(employeeId, CancellationToken.None);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var resultList = okResult.Value as IReadOnlyList<EmployeeDepartmentHistoryModel>;
            resultList.Should().NotBeNull();
            resultList!.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task GetHistoryAsync_propagates_key_not_found_exception_for_middleware_to_handleAsync()
    {
        const int employeeId = 999;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeDepartmentHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Employee with ID {employeeId} not found."));

        Func<Task> act = async () => await _sut.GetHistoryAsync(employeeId, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Employee with ID {employeeId} not found.");
    }
}
