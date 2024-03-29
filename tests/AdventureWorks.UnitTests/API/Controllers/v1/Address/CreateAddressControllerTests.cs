﻿using System.Collections;
using AdventureWorks.API.Controllers.v1.Address;
using AdventureWorks.Application.Interfaces.Services.Address;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using AdventureWorks.Domain.Models;
using AdventureWorks.Domain.Models.Slim;
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
            _ = ((Action)(() => _ = new CreateAddressController(null!, _mockCreateAddressService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("createAddressService");
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
    public async Task PostAsync_invalid_input_returns_bad_requestAsync()
    {
        var input = new AddressCreateModel
        {
            AddressLine1 = "hello World",
            PostalCode = "123",
            AddressStateProvince = new GenericSlimModel { Id = 15 }
        };

        _mockCreateAddressService
            .Setup(x => x.CreateAsync(It.IsAny<AddressCreateModel>()))
            .ReturnsAsync((new AddressModel(),
                new List<ValidationFailure> { new() { PropertyName = "Id", ErrorCode = "00010", ErrorMessage = "Hello Validation Error" } }));

        var result = await _sut.PostAsync(input);

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

        var input = new AddressCreateModel
        {
            AddressLine1 = "hello World",
            PostalCode = "123",
            AddressStateProvince = new GenericSlimModel { Id = 15}
        };

        _mockCreateAddressService
            .Setup(x => x.CreateAsync(It.IsAny<AddressCreateModel>()))
            .ReturnsAsync((addressModel,
                new List<ValidationFailure>()));

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