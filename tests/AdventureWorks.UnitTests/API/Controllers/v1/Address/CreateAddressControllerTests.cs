using AdventureWorks.API.Controllers.v1.Address;
using AdventureWorks.Application.Interfaces.Services.Address;
using AdventureWorks.UnitTests.Setup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using AdventureWorks.Domain.Models;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Address;

[ExcludeFromCodeCoverage]
public sealed class CreateAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateAddressController>> _mockLogger = new();
    private readonly Mock<ICreateAddressService> _mockCreateAddressService = new();
    private readonly CreateAddressController _sut;

    public CreateAddressControllerTests()
    {
        _sut = new CreateAddressController(_mockLogger.Object, _mockCreateAddressService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => new CreateAddressController(null, _mockCreateAddressService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => new CreateAddressController(_mockLogger.Object, null)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("createAddressService");
        }
    }

    [Fact]
    public async Task PostAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PostAsync(null).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be("The address input model cannot be null.");
        }
    }

    [Fact]
    public async Task PostAsync_invalid_input_returns_bad_requestAsync()
    {
        var input = new AddressCreateModel()
        {
            AddressLine1 = "hello World",
            PostalCode = "123",
            StateProvince = new StateProvinceModel { Id = 15 }
        };

        _mockCreateAddressService
            .Setup(x => x.CreateAsync(It.IsAny<AddressCreateModel>()))
            .ReturnsAsync((new AddressModel(),
                new List<ValidationFailure> { new() { PropertyName = "Id", ErrorCode = "00010", ErrorMessage = "Hello Validation Error"} }));

        var result = await _sut.PostAsync(input).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }


    [Fact]
    public async Task PostAsync_invalid_input_returns_createdAsync()
    {
        var addressModel = new AddressModel()
        {
            Id = 1,
            AddressLine1 = "hello World",
            PostalCode = "123"
        };

        var input = new AddressCreateModel()
        {
            AddressLine1 = "hello World",
            PostalCode = "123",
            StateProvince = new StateProvinceModel{Id = 15}
        };

        _mockCreateAddressService
            .Setup(x => x.CreateAsync(It.IsAny<AddressCreateModel>()))
            .ReturnsAsync((addressModel,
                new List<ValidationFailure>()));

        var result = await _sut.PostAsync(input).ConfigureAwait(false);
        
        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();

            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);

            createdResult!.RouteName.Should().Be("GetAddressById");
        }

    }
}