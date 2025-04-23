using AdventureWorks.API.Controllers.v1.Address;
using AdventureWorks.Application.Features.AddressManagement.Commands;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Slim;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Address;

[ExcludeFromCodeCoverage]
public sealed class CreateAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateAddressController _sut;

    public CreateAddressControllerTests()
    {
        _sut = new CreateAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PostAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PostAsync(null);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be("The address input model cannot be null.");
        }
    }

    [Fact]
    public async Task PostAsync_invalid_input_handles_exceptionAsync()
    {
        var input = new AddressCreateModel
        {
            AddressLine1 = "hello World",
            PostalCode = "123",
            AddressStateProvince = new GenericSlimModel { Id = 15, Name = string.Empty, Code = string.Empty }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateAddressCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Id", ErrorCode = "00010", ErrorMessage = "Hello Validation Error" } }));

        Func<Task> act = async () => await _sut.PostAsync(input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PostAsync_valid_input_returns_createdAsync()
    {
        var addressModel = new AddressModel
        {
            Id = 1,
            AddressLine1 = "hello World",
            PostalCode = "123"
        };

        var input = new AddressCreateModel
        {
            AddressLine1 = "hello World",
            PostalCode = "123",
            AddressStateProvince = new GenericSlimModel {Id = 15, Name = string.Empty, Code = string.Empty }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateAddressCommand>(), CancellationToken.None))
            .ReturnsAsync(1);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadAddressQuery>(), CancellationToken.None))
            .ReturnsAsync(addressModel);

        var result = await _sut.PostAsync(input);
        
        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();

            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);

            createdResult!.RouteName.Should().Be("GetAddressById");
        }

    }
}