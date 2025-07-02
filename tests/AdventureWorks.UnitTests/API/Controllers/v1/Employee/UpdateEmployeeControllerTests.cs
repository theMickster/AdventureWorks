using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class UpdateEmployeeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateEmployeeController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateEmployeeController _sut;

    public UpdateEmployeeControllerTests()
    {
        _sut = new UpdateEmployeeController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateEmployeeController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateEmployeeController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PutAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(100, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee input model cannot be null.");
        }
    }

    [Fact]
    public async Task PutAsync_mismatched_ids_returns_bad_requestAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);

        var result = await _sut.PutAsync(200, model);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The ID in the route must match the ID in the request body.");
        }
    }

    [Fact]
    public async Task PutAsync_succeeds_returns_ok_with_updated_modelAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);
        var updatedEmployeeModel = new EmployeeModel
        {
            Id = 100,
            FirstName = model.FirstName,
            LastName = model.LastName,
            JobTitle = "Test Job",
            MaritalStatus = model.MaritalStatus,
            Gender = model.Gender,
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        var result = await _sut.PutAsync(100, model);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeEquivalentTo(updatedEmployeeModel);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<UpdateEmployeeCommand>(cmd =>
                cmd.Model == model &&
                cmd.Model.Id == 100),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMediator.Verify(
            x => x.Send(It.Is<ReadEmployeeQuery>(q => q.BusinessEntityId == 100), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PutAsync_employee_not_found_throws_exceptionAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(999);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 999 not found."));

        Func<Task> act = async () => await _sut.PutAsync(999, model);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public void PutAsync_validation_exception_propagates()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "FirstName", ErrorCode = "Rule-03", ErrorMessage = "First name is required" }
            }));

        Func<Task> act = async () => await _sut.PutAsync(100, model);

        act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PutAsync_sends_correct_command_to_mediator()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);
        UpdateEmployeeCommand? capturedCommand = null;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Unit>, CancellationToken>((cmd, _) => capturedCommand = cmd as UpdateEmployeeCommand)
            .Returns(Task.FromResult(Unit.Value));

        await _sut.PutAsync(100, model);

        using (new AssertionScope())
        {
            capturedCommand.Should().NotBeNull();
            capturedCommand!.Model.Should().Be(model);
            capturedCommand.Model.Id.Should().Be(100);
            capturedCommand.Model.FirstName.Should().Be(model.FirstName);
            capturedCommand.Model.LastName.Should().Be(model.LastName);
            capturedCommand.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public async Task PutAsync_accepts_various_valid_ids(int employeeId)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(employeeId);
        var updatedEmployeeModel = new EmployeeModel
        {
            Id = employeeId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            JobTitle = "Test Job",
            MaritalStatus = model.MaritalStatus,
            Gender = model.Gender,
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        var result = await _sut.PutAsync(employeeId, model);

        var objectResult = result as OkObjectResult;

        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    #region PatchAsync Tests

    [Fact]
    public async Task PatchAsync_null_patch_document_returns_bad_requestAsync()
    {
        var result = await _sut.PatchAsync(100, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The patch document cannot be null.");
        }
    }

    [Fact]
    public async Task PatchAsync_succeeds_returns_ok_with_updated_modelAsync()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");

        var updatedEmployeeModel = new EmployeeModel
        {
            Id = 100,
            FirstName = "Jane",
            LastName = "Doe",
            JobTitle = "Test Job",
            MaritalStatus = "S",
            Gender = "F",
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        var result = await _sut.PatchAsync(100, patchDocument);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeEquivalentTo(updatedEmployeeModel);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<PatchEmployeeCommand>(cmd =>
                cmd.EmployeeId == 100 &&
                cmd.PatchDocument == patchDocument),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMediator.Verify(
            x => x.Send(It.Is<ReadEmployeeQuery>(q => q.BusinessEntityId == 100), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PatchAsync_employee_not_found_throws_exceptionAsync()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 999 not found."));

        Func<Task> act = async () => await _sut.PatchAsync(999, patchDocument);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Employee with ID 999 not found.");
    }

    [Fact]
    public void PatchAsync_validation_exception_propagates()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "FirstName", ErrorCode = "Rule-03", ErrorMessage = "First name is required" }
            }));

        Func<Task> act = async () => await _sut.PatchAsync(100, patchDocument);

        act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PatchAsync_sends_correct_command_to_mediator()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");
        patchDocument.Replace(x => x.LastName, "Smith");

        var updatedEmployeeModel = new EmployeeModel
        {
            Id = 100,
            FirstName = "Jane",
            LastName = "Smith",
            JobTitle = "Test Job",
            MaritalStatus = "S",
            Gender = "F",
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        PatchEmployeeCommand? capturedCommand = null;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Unit>, CancellationToken>((cmd, _) => capturedCommand = cmd as PatchEmployeeCommand)
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        await _sut.PatchAsync(100, patchDocument);

        using (new AssertionScope())
        {
            capturedCommand.Should().NotBeNull();
            capturedCommand!.EmployeeId.Should().Be(100);
            capturedCommand.PatchDocument.Should().Be(patchDocument);
            capturedCommand.PatchDocument.Operations.Should().HaveCount(2);
            capturedCommand.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public async Task PatchAsync_accepts_various_valid_ids(int employeeId)
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Test Name");

        var updatedEmployeeModel = new EmployeeModel
        {
            Id = employeeId,
            FirstName = "Test Name",
            LastName = "Doe",
            JobTitle = "Test Job",
            MaritalStatus = "S",
            Gender = "M",
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        var result = await _sut.PatchAsync(employeeId, patchDocument);

        var objectResult = result as OkObjectResult;

        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task PatchAsync_handles_multiple_operations()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");
        patchDocument.Replace(x => x.LastName, "Smith");
        patchDocument.Replace(x => x.MaritalStatus, "M");

        var updatedEmployeeModel = new EmployeeModel
        {
            Id = 100,
            FirstName = "Jane",
            LastName = "Smith",
            JobTitle = "Test Job",
            MaritalStatus = "M",
            Gender = "F",
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        var result = await _sut.PatchAsync(100, patchDocument);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeEquivalentTo(updatedEmployeeModel);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<PatchEmployeeCommand>(cmd =>
                cmd.PatchDocument.Operations.Count == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMediator.Verify(
            x => x.Send(It.Is<ReadEmployeeQuery>(q => q.BusinessEntityId == 100), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PatchAsync_handles_single_field_update()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.MaritalStatus, "M");

        var updatedEmployeeModel = new EmployeeModel
        {
            Id = 100,
            FirstName = "John",
            LastName = "Doe",
            JobTitle = "Test Job",
            MaritalStatus = "M",
            Gender = "M",
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        var result = await _sut.PatchAsync(100, patchDocument);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeEquivalentTo(updatedEmployeeModel);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<PatchEmployeeCommand>(cmd =>
                cmd.EmployeeId == 100 &&
                cmd.PatchDocument.Operations.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMediator.Verify(
            x => x.Send(It.Is<ReadEmployeeQuery>(q => q.BusinessEntityId == 100), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PatchAsync_handles_all_patchable_fields()
    {
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Jane");
        patchDocument.Replace(x => x.LastName, "Smith");
        patchDocument.Replace(x => x.MiddleName, "Marie");
        patchDocument.Replace(x => x.Title, "Ms.");
        patchDocument.Replace(x => x.Suffix, "Jr.");
        patchDocument.Replace(x => x.MaritalStatus, "M");
        patchDocument.Replace(x => x.Gender, "F");

        var updatedEmployeeModel = new EmployeeModel
        {
            Id = 100,
            FirstName = "Jane",
            LastName = "Smith",
            MiddleName = "Marie",
            Title = "Ms.",
            Suffix = "Jr.",
            JobTitle = "Test Job",
            MaritalStatus = "M",
            Gender = "F",
            NationalIdNumber = "123456789",
            LoginId = "test\\login"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        var result = await _sut.PatchAsync(100, patchDocument);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeEquivalentTo(updatedEmployeeModel);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<PatchEmployeeCommand>(cmd =>
                cmd.PatchDocument.Operations.Count == 7),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMediator.Verify(
            x => x.Send(It.Is<ReadEmployeeQuery>(q => q.BusinessEntityId == 100), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PatchAsync_sends_query_with_correct_idAsync()
    {
        var expectedEmployeeId = 555;
        var patchDocument = new JsonPatchDocument<EmployeeUpdateModel>();
        patchDocument.Replace(x => x.FirstName, "Test");

        var updatedEmployeeModel = new EmployeeModel
        {
            Id = expectedEmployeeId,
            FirstName = "Test",
            LastName = "User",
            JobTitle = "Engineer",
            MaritalStatus = "S",
            Gender = "M",
            NationalIdNumber = "987654321",
            LoginId = "test\\user"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployeeModel);

        await _sut.PatchAsync(expectedEmployeeId, patchDocument);

        _mockMediator.Verify(
            x => x.Send(It.Is<ReadEmployeeQuery>(q => q.BusinessEntityId == expectedEmployeeId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
