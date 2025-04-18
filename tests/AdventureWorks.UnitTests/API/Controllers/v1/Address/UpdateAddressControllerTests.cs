using System.Collections;
using AdventureWorks.API.Controllers.v1.Address;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using AdventureWorks.Models.Features.AddressManagement;
using FluentValidation.Results;
using AdventureWorks.Application.Features.AddressManagement.Contracts;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Address;

[ExcludeFromCodeCoverage]
public sealed class UpdateAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateAddressController>> _mockLogger = new();
    private readonly Mock<IUpdateAddressService> _mockUpdateAddressService = new();
    private readonly UpdateAddressController _sut;

    public UpdateAddressControllerTests()
    {
        _sut = new UpdateAddressController(_mockLogger.Object, _mockUpdateAddressService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateAddressController(null!, _mockUpdateAddressService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("updateAddressService");
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
    public async Task PutAsyn_mismatched_ids_returns_bad_requestAsync()
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
    public async Task PutAsync_input_has_validation_errors_returns_bad_requestAsync()
    {
        _mockUpdateAddressService.Setup(x => x.UpdateAsync(It.IsAny<AddressUpdateModel>()))
            .ReturnsAsync((new AddressModel(),
                new List<ValidationFailure>
                    { new() { PropertyName = "Id", ErrorCode = "00010", ErrorMessage = "Hello Validation Error" } }));

        var result = await _sut.PutAsync(11, new AddressUpdateModel { Id = 11 });

        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as IEnumerable;
        
        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().NotBeNullOrWhiteSpace();

            outputModel.Should().NotBeNull();
            outputModel?.Cast<string>().Select(x => x).FirstOrDefault().Should().Be("Hello Validation Error");
        }
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

        _mockUpdateAddressService.Setup(x => x.UpdateAsync(It.IsAny<AddressUpdateModel>()))
            .ReturnsAsync((addressModel,
                new List<ValidationFailure>()));

        var result = await _sut.PutAsync(11, new AddressUpdateModel { Id = 11 });

        var objectResult = result as OkObjectResult;
        var outputModel = objectResult!.Value! as AddressModel;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult!.Value!.ToString().Should().NotBeNullOrWhiteSpace();

            outputModel.Should().NotBeNull();
            outputModel!.Id.Should().Be(1);
        }
    }
}