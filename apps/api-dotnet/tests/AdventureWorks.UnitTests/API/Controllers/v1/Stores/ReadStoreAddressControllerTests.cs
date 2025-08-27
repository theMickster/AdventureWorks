using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Stores;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadStoreAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadStoreAddressController _sut;

    public ReadStoreAddressControllerTests()
    {
        _sut = new ReadStoreAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadStoreAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadStoreAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetAllAsync_invalid_storeId_returns_bad_request(int storeId)
    {
        var result = await _sut.GetAllAsync(storeId);
        var objectResult = result as BadRequestObjectResult;
        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be("A valid store id must be specified.");
        }
    }

    [Fact]
    public async Task GetAllAsync_returns_200_with_address_listAsync()
    {
        var addresses = new List<StoreAddressModel>
        {
            new() { Id = 1, AddressTypeName = "Home" },
            new() { Id = 2, AddressTypeName = "Billing" }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        var result = await _sut.GetAllAsync(2534);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var returnedList = objectResult.Value as List<StoreAddressModel>;
            returnedList.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetAllAsync_returns_200_with_empty_listAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoreAddressModel>());

        var result = await _sut.GetAllAsync(9999);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var returnedList = objectResult.Value as List<StoreAddressModel>;
            returnedList.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetAllAsync_sends_correct_storeId_to_mediatorAsync()
    {
        const int storeId = 2534;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoreAddressModel>());

        await _sut.GetAllAsync(storeId);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreAddressListQuery>(q => q.StoreId == storeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 100, 2, "A valid store id must be specified.")]
    [InlineData(-1, 100, 2, "A valid store id must be specified.")]
    [InlineData(2534, 0, 2, "A valid address id must be specified.")]
    [InlineData(2534, -3, 2, "A valid address id must be specified.")]
    [InlineData(2534, 100, 0, "A valid address type id must be specified.")]
    [InlineData(2534, 100, -7, "A valid address type id must be specified.")]
    public async Task GetByCompositeKeyAsync_invalid_route_values_return_bad_requestAsync(int storeId, int addressId, int addressTypeId, string expectedMessage)
    {
        var result = await _sut.GetByCompositeKeyAsync(storeId, addressId, addressTypeId);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value!.ToString().Should().Be(expectedMessage);
        }
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_returns_not_found_when_query_returns_nullAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreAddressModel?)null);

        var result = await _sut.GetByCompositeKeyAsync(2534, 100, 2);

        var notFound = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            notFound!.Value!.ToString().Should().Be("Unable to locate the store address.");
        }
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_returns_ok_with_modelAsync()
    {
        var output = new StoreAddressModel
        {
            Id = 100,
            StoreId = 2534,
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

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(output);

        var result = await _sut.GetByCompositeKeyAsync(2534, 100, 2);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().Be(output);
        }
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_sends_correct_composite_key_to_mediatorAsync()
    {
        const int storeId = 2534;
        const int addressId = 100;
        const int addressTypeId = 2;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreAddressModel?)null);

        await _sut.GetByCompositeKeyAsync(storeId, addressId, addressTypeId);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreAddressQuery>(q =>
                q.StoreId == storeId && q.AddressId == addressId && q.AddressTypeId == addressTypeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
