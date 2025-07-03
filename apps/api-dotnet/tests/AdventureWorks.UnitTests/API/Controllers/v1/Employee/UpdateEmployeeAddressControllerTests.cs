using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Commands;
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
public sealed class UpdateEmployeeAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateEmployeeAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateEmployeeAddressController _sut;

    public UpdateEmployeeAddressControllerTests()
    {
        _sut = new UpdateEmployeeAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateEmployeeAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateEmployeeAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    #region PutAsync Tests

    [Fact]
    public async Task PutAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(100, 1, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The address input model cannot be null.");
        }
    }

    [Fact]
    public async Task PutAsync_mismatched_ids_returns_bad_requestAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(1);

        var result = await _sut.PutAsync(100, 5, model);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The address ID in the route must match the ID in the request body.");
        }
    }

    [Fact]
    public async Task PutAsync_succeeds_returns_no_contentAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.PutAsync(100, 1, model);

        var objectResult = result as NoContentResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<UpdateEmployeeAddressCommand>(cmd =>
                cmd.BusinessEntityId == 100 &&
                cmd.Model == model),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PutAsync_not_found_returns_not_foundAsync()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(999);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Address with ID 999 not found for employee 100."));

        var result = await _sut.PutAsync(100, 999, model);

        var objectResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            objectResult.Value.Should().Be("Address with ID 999 not found for employee 100.");
        }
    }

    [Fact]
    public void PutAsync_validation_exception_propagates()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "City", ErrorCode = "Rule-06", ErrorMessage = "City is required" }
            }));

        Func<Task> act = async () => await _sut.PutAsync(100, 1, model);

        act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PutAsync_sends_correct_command_to_mediator()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(1);
        UpdateEmployeeAddressCommand? capturedCommand = null;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Unit>, CancellationToken>((cmd, _) => capturedCommand = cmd as UpdateEmployeeAddressCommand)
            .Returns(Task.FromResult(Unit.Value));

        await _sut.PutAsync(100, 1, model);

        using (new AssertionScope())
        {
            capturedCommand.Should().NotBeNull();
            capturedCommand!.BusinessEntityId.Should().Be(100);
            capturedCommand.Model.Should().Be(model);
            capturedCommand.Model.AddressId.Should().Be(1);
            capturedCommand.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 5)]
    [InlineData(9999, 99)]
    public async Task PutAsync_accepts_various_valid_ids(int employeeId, int addressId)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeAddressUpdateModel(addressId);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.PutAsync(employeeId, addressId, model);

        var objectResult = result as NoContentResult;

        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    #endregion

    #region PatchAsync Tests

    [Fact]
    public async Task PatchAsync_null_patch_document_returns_bad_requestAsync()
    {
        var result = await _sut.PatchAsync(100, 1, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The patch document cannot be null.");
        }
    }

    [Fact]
    public async Task PatchAsync_succeeds_returns_no_contentAsync()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "Portland");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.PatchAsync(100, 1, patchDocument);

        var objectResult = result as NoContentResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<PatchEmployeeAddressCommand>(cmd =>
                cmd.BusinessEntityId == 100 &&
                cmd.AddressId == 1 &&
                cmd.PatchDocument == patchDocument),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PatchAsync_not_found_returns_not_foundAsync()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "Portland");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Employee with ID 100 not found."));

        var result = await _sut.PatchAsync(100, 1, patchDocument);

        var objectResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            objectResult.Value.Should().Be("Employee with ID 100 not found.");
        }
    }

    [Fact]
    public void PatchAsync_validation_exception_propagates()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "City", ErrorCode = "Rule-06", ErrorMessage = "City is required" }
            }));

        Func<Task> act = async () => await _sut.PatchAsync(100, 1, patchDocument);

        act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PatchAsync_sends_correct_command_to_mediator()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "Seattle");
        patchDocument.Replace(x => x.PostalCode, "98101");

        PatchEmployeeAddressCommand? capturedCommand = null;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Unit>, CancellationToken>((cmd, _) => capturedCommand = cmd as PatchEmployeeAddressCommand)
            .Returns(Task.FromResult(Unit.Value));

        await _sut.PatchAsync(100, 1, patchDocument);

        using (new AssertionScope())
        {
            capturedCommand.Should().NotBeNull();
            capturedCommand!.BusinessEntityId.Should().Be(100);
            capturedCommand.AddressId.Should().Be(1);
            capturedCommand.PatchDocument.Should().Be(patchDocument);
            capturedCommand.PatchDocument.Operations.Should().HaveCount(2);
            capturedCommand.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 5)]
    [InlineData(9999, 99)]
    public async Task PatchAsync_accepts_various_valid_ids(int employeeId, int addressId)
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.City, "Test City");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.PatchAsync(employeeId, addressId, patchDocument);

        var objectResult = result as NoContentResult;

        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PatchAsync_handles_multiple_operations()
    {
        var patchDocument = new JsonPatchDocument<EmployeeAddressUpdateModel>();
        patchDocument.Replace(x => x.AddressLine1, "456 Oak Avenue");
        patchDocument.Replace(x => x.City, "Portland");
        patchDocument.Replace(x => x.PostalCode, "97201");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<PatchEmployeeAddressCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.PatchAsync(100, 1, patchDocument);

        var objectResult = result as NoContentResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        _mockMediator.Verify(
            x => x.Send(It.Is<PatchEmployeeAddressCommand>(cmd =>
                cmd.PatchDocument.Operations.Count == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
