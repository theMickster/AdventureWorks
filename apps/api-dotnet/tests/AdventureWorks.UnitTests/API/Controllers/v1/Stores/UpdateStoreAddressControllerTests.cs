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
public sealed class UpdateStoreAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateStoreAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateStoreAddressController _sut;

    public UpdateStoreAddressControllerTests()
    {
        _sut = new UpdateStoreAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateStoreAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateStoreAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PatchAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PatchAsync(2534, 100, 2, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("The store address input model cannot be null.");
        }
    }

    [Theory]
    [InlineData(0, 100, 2, "A valid store id must be specified.")]
    [InlineData(-1, 100, 2, "A valid store id must be specified.")]
    [InlineData(2534, 0, 2, "A valid address id must be specified.")]
    [InlineData(2534, -5, 2, "A valid address id must be specified.")]
    [InlineData(2534, 100, 0, "A valid address type id must be specified.")]
    [InlineData(2534, 100, -2, "A valid address type id must be specified.")]
    public async Task PatchAsync_invalid_route_values_return_bad_requestAsync(int storeId, int addressId, int addressTypeId, string expectedMessage)
    {
        var input = new StoreAddressUpdateModel { AddressTypeId = 3 };

        var result = await _sut.PatchAsync(storeId, addressId, addressTypeId, input);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be(expectedMessage);
        }
    }

    [Fact]
    public async Task PatchAsync_propagates_ValidationExceptionAsync()
    {
        var input = new StoreAddressUpdateModel { AddressTypeId = 0 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreAddressTypeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
            {
                new() { PropertyName = "AddressTypeId", ErrorCode = "Rule-01", ErrorMessage = "Bad type" }
            }));

        Func<Task> act = async () => await _sut.PatchAsync(2534, 100, 2, input);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PatchAsync_propagates_KeyNotFoundException_when_address_missingAsync()
    {
        var input = new StoreAddressUpdateModel { AddressTypeId = 3 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreAddressTypeCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Address not found"));

        Func<Task> act = async () => await _sut.PatchAsync(2534, 100, 2, input);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task PatchAsync_returns_not_found_when_post_update_read_returns_nullAsync()
    {
        var input = new StoreAddressUpdateModel { AddressTypeId = 3 };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreAddressTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadStoreAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreAddressModel?)null);

        var result = await _sut.PatchAsync(2534, 100, 2, input);

        var notFound = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task PatchAsync_valid_input_returns_okAsync()
    {
        const int storeId = 2534;
        var input = new StoreAddressUpdateModel { AddressTypeId = 3 };

        var output = new StoreAddressModel
        {
            Id = 100,
            StoreId = storeId,
            AddressTypeId = 3,
            AddressTypeName = "Shipping",
            AddressLine1 = "123 Main St",
            City = "Seattle",
            StateProvinceCode = "WA",
            StateProvinceName = "Washington",
            CountryRegionCode = "US",
            CountryRegionName = "United States",
            PostalCode = "98101"
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateStoreAddressTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadStoreAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.PatchAsync(storeId, 100, 2, input);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returned = okResult!.Value as StoreAddressModel;
            returned.Should().NotBeNull();
            returned!.AddressTypeId.Should().Be(3);
            returned!.AddressTypeName.Should().Be("Shipping");
        }
    }
}
