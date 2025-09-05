using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Exceptions;
using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class EmployeeLifecycleControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<EmployeeLifecycleController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly EmployeeLifecycleController _sut;

    public EmployeeLifecycleControllerTests()
    {
        _sut = new EmployeeLifecycleController(_mockLogger.Object, _mockMediator.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new EmployeeLifecycleController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new EmployeeLifecycleController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    #endregion

    #region HireAsync Tests

    [Fact]
    public async Task HireAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.HireAsync(1, null);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The hire input model cannot be null.");
        }
    }

    [Fact]
    public async Task HireAsync_mismatched_employee_id_returns_bad_requestAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);

        var result = await _sut.HireAsync(999, input); // Route ID doesn't match model ID

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee ID in the route must match the employee ID in the request body.");
        }
    }

    [Fact]
    public async Task HireAsync_employee_not_found_returns_not_foundAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<HireEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 1 not found."));

        var result = await _sut.HireAsync(1, input);

        var notFoundResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFoundResult.Value.Should().Be("Employee with ID 1 not found.");
        }
    }

    [Fact]
    public async Task HireAsync_already_active_employee_returns_bad_requestAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<HireEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("Employee 1 is already active and cannot be hired again."));

        var result = await _sut.HireAsync(1, input);

        var badRequestResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            badRequestResult.Value.Should().Be("Employee 1 is already active and cannot be hired again.");
        }
    }

    [Fact]
    public async Task HireAsync_validation_exception_is_propagatedAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<HireEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "InitialPayRate", ErrorCode = "Rule-50", ErrorMessage = "PayRate cannot exceed $500.00." } }));

        Func<Task> act = async () => await _sut.HireAsync(1, input);

        _ = await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HireAsync_valid_input_returns_ok_with_business_entity_idAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeHireModel(employeeId: 1);
        const int expectedBusinessEntityId = 1;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<HireEmployeeCommand>(), CancellationToken.None))
            .ReturnsAsync(expectedBusinessEntityId);

        var result = await _sut.HireAsync(1, input);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().NotBeNull();

            // Use reflection to access anonymous object properties
            var valueType = okResult.Value!.GetType();
            var businessEntityIdProp = valueType.GetProperty("businessEntityId");
            var messageProp = valueType.GetProperty("message");

            businessEntityIdProp.Should().NotBeNull();
            messageProp.Should().NotBeNull();

            var businessEntityId = (int)businessEntityIdProp!.GetValue(okResult.Value)!;
            var message = (string)messageProp!.GetValue(okResult.Value)!;

            businessEntityId.Should().Be(expectedBusinessEntityId);
            message.Should().Be("Employee hired successfully");
        }
    }

    #endregion

    #region TerminateAsync Tests

    [Fact]
    public async Task TerminateAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.TerminateAsync(1, null);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The termination input model cannot be null.");
        }
    }

    [Fact]
    public async Task TerminateAsync_mismatched_employee_id_returns_bad_requestAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);

        var result = await _sut.TerminateAsync(999, input);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee ID in the route must match the employee ID in the request body.");
        }
    }

    [Fact]
    public async Task TerminateAsync_employee_not_found_returns_not_foundAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<TerminateEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 1 not found."));

        var result = await _sut.TerminateAsync(1, input);

        var notFoundResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFoundResult.Value.Should().Be("Employee with ID 1 not found.");
        }
    }

    [Fact]
    public async Task TerminateAsync_already_terminated_employee_returns_bad_requestAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<TerminateEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("Employee 1 is already terminated."));

        var result = await _sut.TerminateAsync(1, input);

        var badRequestResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            badRequestResult.Value.Should().Be("Employee 1 is already terminated.");
        }
    }

    [Fact]
    public async Task TerminateAsync_validation_exception_is_propagatedAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<TerminateEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Reason", ErrorCode = "Rule-Reason", ErrorMessage = "Reason is required and cannot be empty." } }));

        Func<Task> act = async () => await _sut.TerminateAsync(1, input);

        _ = await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task TerminateAsync_valid_input_returns_ok_with_success_messageAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeTerminateModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<TerminateEmployeeCommand>(), CancellationToken.None))
            .ReturnsAsync(Unit.Value);

        var result = await _sut.TerminateAsync(1, input);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            okResult.Value.Should().NotBeNull();

            // Use reflection to access anonymous object properties
            var valueType = okResult.Value!.GetType();
            var messageProp = valueType.GetProperty("message");
            messageProp.Should().NotBeNull();

            var message = (string)messageProp!.GetValue(okResult.Value)!;
            message.Should().Be("Employee terminated successfully");
        }
    }

    #endregion

    #region RehireAsync Tests

    [Fact]
    public async Task RehireAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.RehireAsync(1, null);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The rehire input model cannot be null.");
        }
    }

    [Fact]
    public async Task RehireAsync_mismatched_employee_id_returns_bad_requestAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);

        var result = await _sut.RehireAsync(999, input);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee ID in the route must match the employee ID in the request body.");
        }
    }

    [Fact]
    public async Task RehireAsync_employee_not_found_returns_not_foundAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RehireEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 1 not found."));

        var result = await _sut.RehireAsync(1, input);

        var notFoundResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFoundResult.Value.Should().Be("Employee with ID 1 not found.");
        }
    }

    [Fact]
    public async Task RehireAsync_already_active_employee_returns_bad_requestAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RehireEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("Employee 1 is already active. Use department transfer instead of rehire."));

        var result = await _sut.RehireAsync(1, input);

        var badRequestResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            badRequestResult.Value.Should().Be("Employee 1 is already active. Use department transfer instead of rehire.");
        }
    }

    [Fact]
    public async Task RehireAsync_within_90_days_returns_bad_requestAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RehireEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("Employee 1 cannot be rehired until 90 days after termination. Last termination: 2024-10-15. Earliest rehire date: 2025-01-13"));

        var result = await _sut.RehireAsync(1, input);

        var badRequestResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            badRequestResult.Value.Should().NotBeNull();
            badRequestResult.Value.ToString().Should().Contain("cannot be rehired until 90 days after termination");
        }
    }

    [Fact]
    public async Task RehireAsync_validation_exception_is_propagatedAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RehireEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "PayRate", ErrorCode = "Rule-50", ErrorMessage = "PayRate cannot exceed $500.00." } }));

        Func<Task> act = async () => await _sut.RehireAsync(1, input);

        _ = await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RehireAsync_valid_input_returns_ok_with_business_entity_idAsync()
    {
        var input = HumanResourcesDomainFixtures.GetValidEmployeeRehireModel(employeeId: 1);
        const int expectedBusinessEntityId = 1;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RehireEmployeeCommand>(), CancellationToken.None))
            .ReturnsAsync(expectedBusinessEntityId);

        var result = await _sut.RehireAsync(1, input);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            okResult.Value.Should().NotBeNull();

            // Use reflection to access anonymous object properties
            var valueType = okResult.Value!.GetType();
            var businessEntityIdProp = valueType.GetProperty("businessEntityId");
            var messageProp = valueType.GetProperty("message");

            businessEntityIdProp.Should().NotBeNull();
            messageProp.Should().NotBeNull();

            var businessEntityId = (int)businessEntityIdProp!.GetValue(okResult.Value)!;
            var message = (string)messageProp!.GetValue(okResult.Value)!;

            businessEntityId.Should().Be(expectedBusinessEntityId);
            message.Should().Be("Employee rehired successfully");
        }
    }

    #endregion

    #region TransferAsync Tests

    [Fact]
    public void TransferAsync_controller_has_authorize_attribute()
    {
        var attribute = typeof(EmployeeLifecycleController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .FirstOrDefault() as AuthorizeAttribute;

        attribute.Should().NotBeNull("because the controller must require authentication for all actions");
    }

    [Fact]
    public async Task TransferAsync_returns_bad_request_when_employee_id_is_zeroAsync()
    {
        var result = await _sut.TransferAsync(0, new EmployeeTransferModel(), CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task TransferAsync_returns_bad_request_when_employee_id_is_negativeAsync()
    {
        var result = await _sut.TransferAsync(-1, new EmployeeTransferModel(), CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task TransferAsync_returns_bad_request_when_model_is_nullAsync()
    {
        var result = await _sut.TransferAsync(1, null, CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The transfer input model cannot be null.");
        }
    }

    [Fact]
    public async Task TransferAsync_employee_not_found_returns_not_foundAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<TransferEmployeeDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 1 not found."));

        var model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 1 };

        var result = await _sut.TransferAsync(1, model, CancellationToken.None);

        var notFoundResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFoundResult.Value.Should().Be("Employee with ID 1 not found.");
        }
    }

    [Fact]
    public async Task TransferAsync_propagates_conflict_exception_for_middleware_to_handleAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<TransferEmployeeDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Employee 1 has no active department assignment to close."));

        var model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 1 };

        Func<Task> act = async () => await _sut.TransferAsync(1, model, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task TransferAsync_returns_201_created_at_route_on_successAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<TransferEmployeeDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var model = new EmployeeTransferModel { DepartmentId = 3, ShiftId = 1 };

        var result = await _sut.TransferAsync(1, model, CancellationToken.None);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult.RouteName.Should().Be("GetEmployeeDepartmentHistory");
            createdResult.RouteValues.Should().ContainKey("version")
                .WhoseValue.Should().Be("1.0");
            createdResult.RouteValues.Should().ContainKey("employeeId")
                .WhoseValue.Should().Be(1);
        }
    }

    #endregion

    #region RecordPayChangeAsync Tests

    [Fact]
    public async Task RecordPayChange_employeeId_zero_returns_BadRequestAsync()
    {
        var result = await _sut.RecordPayChangeAsync(0, new EmployeePayChangeCreateModel(), CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task RecordPayChange_employeeId_negative_returns_BadRequestAsync()
    {
        var result = await _sut.RecordPayChangeAsync(-1, new EmployeePayChangeCreateModel(), CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task RecordPayChange_null_model_returns_BadRequestAsync()
    {
        var result = await _sut.RecordPayChangeAsync(1, null, CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The pay change input model cannot be null.");
        }
    }

    [Fact]
    public async Task RecordPayChange_KeyNotFoundException_returns_NotFoundAsync()
    {
        const string message = "Employee with ID 999 not found.";

        _mockMediator
            .Setup(x => x.Send(It.IsAny<RecordEmployeePayChangeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException(message));

        var model = new EmployeePayChangeCreateModel { Rate = 50m, PayFrequency = 2 };
        var result = await _sut.RecordPayChangeAsync(999, model, CancellationToken.None);

        var notFoundResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFoundResult.Value.Should().Be(message);
        }
    }

    [Fact]
    public async Task RecordPayChange_happy_path_returns_201_with_correct_routeAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<RecordEmployeePayChangeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var model = new EmployeePayChangeCreateModel { Rate = 50m, PayFrequency = 2 };
        var result = await _sut.RecordPayChangeAsync(1, model, CancellationToken.None);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult.RouteName.Should().Be("GetEmployeePayHistory");
            createdResult.RouteValues.Should().ContainKey("version")
                .WhoseValue.Should().Be("1.0");
            createdResult.RouteValues.Should().ContainKey("employeeId")
                .WhoseValue.Should().Be(1);
        }
    }

    #endregion

    #region GetLifecycleStatusAsync Tests

    [Fact]
    public async Task GetLifecycleStatusAsync_invalid_employee_id_returns_bad_requestAsync()
    {
        var result = await _sut.GetLifecycleStatusAsync(0);

        var badRequestResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            badRequestResult.Value.Should().Be("A valid employee ID must be specified.");
        }
    }

    [Fact]
    public async Task GetLifecycleStatusAsync_negative_employee_id_returns_bad_requestAsync()
    {
        var result = await _sut.GetLifecycleStatusAsync(-1);

        var badRequestResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            badRequestResult.Value.Should().Be("A valid employee ID must be specified.");
        }
    }

    [Fact]
    public async Task GetLifecycleStatusAsync_employee_not_found_returns_not_foundAsync()
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeLifecycleStatusQuery>(), CancellationToken.None))
            .ReturnsAsync((EmployeeLifecycleStatusModel?)null);

        var result = await _sut.GetLifecycleStatusAsync(999);

        var notFoundResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFoundResult.Value.Should().Be("Unable to locate the employee.");
        }
    }

    [Fact]
    public async Task GetLifecycleStatusAsync_valid_employee_returns_ok_with_statusAsync()
    {
        var expectedStatus = new EmployeeLifecycleStatusModel
        {
            EmployeeId = 1,
            FullName = "John Doe",
            EmploymentStatus = "Active",
            HireDate = new DateTime(2020, 1, 10),
            TerminationDate = null,
            DaysEmployed = 1500,
            CurrentDepartment = "Engineering",
            CurrentShift = "Day",
            DepartmentStartDate = new DateTime(2023, 6, 1),
            CurrentPayRate = 75.50m,
            PayRateEffectiveDate = new DateTime(2024, 1, 1),
            VacationHoursBalance = 80,
            SickLeaveHoursBalance = 48,
            EligibleForRehire = false,
            RehireCount = 0
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeLifecycleStatusQuery>(), CancellationToken.None))
            .ReturnsAsync(expectedStatus);

        var result = await _sut.GetLifecycleStatusAsync(1);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var value = okResult.Value as EmployeeLifecycleStatusModel;
            value.Should().NotBeNull();
            value!.EmployeeId.Should().Be(1);
            value.FullName.Should().Be("John Doe");
            value.EmploymentStatus.Should().Be("Active");
            value.CurrentDepartment.Should().Be("Engineering");
            value.CurrentPayRate.Should().Be(75.50m);
        }
    }

    [Fact]
    public async Task GetLifecycleStatusAsync_terminated_employee_returns_ok_with_statusAsync()
    {
        var expectedStatus = new EmployeeLifecycleStatusModel
        {
            EmployeeId = 2,
            FullName = "Jane Smith",
            EmploymentStatus = "Terminated",
            HireDate = new DateTime(2018, 3, 15),
            TerminationDate = new DateTime(2024, 10, 31),
            DaysEmployed = 2421,
            CurrentDepartment = null,
            CurrentShift = null,
            DepartmentStartDate = null,
            CurrentPayRate = null,
            PayRateEffectiveDate = null,
            VacationHoursBalance = 0,
            SickLeaveHoursBalance = 0,
            EligibleForRehire = true,
            RehireCount = 1
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeLifecycleStatusQuery>(), CancellationToken.None))
            .ReturnsAsync(expectedStatus);

        var result = await _sut.GetLifecycleStatusAsync(2);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var value = okResult.Value as EmployeeLifecycleStatusModel;
            value.Should().NotBeNull();
            value!.EmployeeId.Should().Be(2);
            value.FullName.Should().Be("Jane Smith");
            value.EmploymentStatus.Should().Be("Terminated");
            value.TerminationDate.Should().Be(new DateTime(2024, 10, 31));
            value.EligibleForRehire.Should().BeTrue();
            value.RehireCount.Should().Be(1);
        }
    }

    #endregion
}
