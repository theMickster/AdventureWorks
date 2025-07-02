using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
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
    public async Task PutAsync_succeeds_returns_no_contentAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(100);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.PutAsync(100, model);

        var objectResult = result as NoContentResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<UpdateEmployeeCommand>(cmd =>
                cmd.Model == model &&
                cmd.Model.Id == 100),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PutAsync_employee_not_found_returns_not_foundAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeUpdateModel(999);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 999 not found."));

        var result = await _sut.PutAsync(999, model);

        var objectResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            objectResult.Value.Should().Be("Employee with ID 999 not found.");
        }
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

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.PutAsync(employeeId, model);

        var objectResult = result as NoContentResult;

        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }
}
