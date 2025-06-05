using AdventureWorks.API.Controllers.v1.Address;
using AdventureWorks.Application.Features.AddressManagement.Commands;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Address;

[ExcludeFromCodeCoverage]
public sealed class UpdateAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateAddressController _sut;

    public UpdateAddressControllerTests()
    {
        _sut = new UpdateAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PutAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(1, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task PutAsync_invalid_id_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(-1, new AddressUpdateModel());

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task PutAsync_mismatched_ids_returns_bad_requestAsync()
    {
        var result = await _sut.PutAsync(11, new AddressUpdateModel{Id = 15});

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void PutAsync_invalid_input_handles_exception()
    {

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateAddressCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Id", ErrorCode = "00010", ErrorMessage = "Hello Validation Error" } }));

        Func<Task> act = async () => await _sut.PutAsync(11, new AddressUpdateModel { Id = 11 });

        _ = act.Should().ThrowAsync<ValidationException>();
    }


    [Fact]
    public async Task PutAsync_succeeds_Async()
    {
        var addressModel = new AddressModel
        {
            Id = 11,
            AddressLine1 = "hello World",
            PostalCode = "123"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateAddressCommand>(), CancellationToken.None));

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addressModel);

        var result = await _sut.PutAsync(11, new AddressUpdateModel { Id = 11 });

        var objectResult = result as OkObjectResult;
        var outputModel = objectResult!.Value! as AddressModel;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult!.Value!.ToString().Should().NotBeNullOrWhiteSpace();

            outputModel.Should().NotBeNull();
            outputModel!.Id.Should().Be(11);
        }
    }
}