using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class CreateStoreAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateStoreAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateStoreAddressController _sut;

    public CreateStoreAddressControllerTests()
    {
        _sut = new CreateStoreAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateStoreAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateStoreAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PostAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PostAsync(2534, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The store address input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task PostAsync_invalid_storeId_returns_bad_requestAsync(int storeId)
    {
        var input = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 };

        var result = await _sut.PostAsync(storeId, input);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid store id must be specified.");
        }
    }

    [Fact]
    public async Task PostAsync_propagates_ValidationExceptionAsync()
    {
        var input = new StoreAddressCreateModel { AddressId = 0, AddressTypeId = 0 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<AddStoreAddressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "AddressId", ErrorCode = "Rule-02", ErrorMessage = "Bad address" }
            }));

        Func<Task> act = async () => await _sut.PostAsync(2534, input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PostAsync_propagates_KeyNotFoundException_when_store_missingAsync()
    {
        var input = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<AddStoreAddressCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Store not found"));

        Func<Task> act = async () => await _sut.PostAsync(9999, input);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task PostAsync_throws_InvalidOperationException_when_post_add_read_returns_nullAsync()
    {
        const int storeId = 2534;
        var input = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<AddStoreAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(input.AddressId);
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadStoreAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreAddressModel?)null);

        Func<Task> act = async () => await _sut.PostAsync(storeId, input);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PostAsync_valid_input_returns_createdAsync()
    {
        const int storeId = 2534;

        var input = new StoreAddressCreateModel { AddressId = 100, AddressTypeId = 2 };

        var output = new StoreAddressModel
        {
            Id = 100,
            StoreId = storeId,
            AddressTypeId = 2,
            AddressTypeName = "Main Office",
            AddressLine1 = "123 Main St",
            City = "Seattle",
            StateProvinceCode = "WA",
            StateProvinceName = "Washington",
            CountryRegionCode = "US",
            CountryRegionName = "United States",
            PostalCode = "98101"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<AddStoreAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(input.AddressId);
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadStoreAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.PostAsync(storeId, input);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult!.RouteName.Should().Be("GetStoreAddressByCompositeKey");

            var routeValues = createdResult!.RouteValues;
            routeValues.Should().ContainKey("storeId");
            routeValues!["storeId"].Should().Be(storeId);
            routeValues.Should().ContainKey("addressId");
            routeValues!["addressId"].Should().Be(input.AddressId);
            routeValues.Should().ContainKey("addressTypeId");
            routeValues!["addressTypeId"].Should().Be(input.AddressTypeId);

            var returned = createdResult.Value as StoreAddressModel;
            returned.Should().NotBeNull();
            returned!.Id.Should().Be(100);
            returned!.StoreId.Should().Be(storeId);
            returned!.AddressTypeId.Should().Be(2);
        }
    }
}
