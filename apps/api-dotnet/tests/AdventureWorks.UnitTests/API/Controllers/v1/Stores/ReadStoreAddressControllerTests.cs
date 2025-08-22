using AdventureWorks.API.Controllers.v1.Stores;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.AddressManagement;
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
        var addresses = new List<BusinessEntityAddressModel>
        {
            new() { Address = new AddressModel { Id = 1 }, AddressType = new AddressTypeModel { Name = "Home" } },
            new() { Address = new AddressModel { Id = 2 }, AddressType = new AddressTypeModel { Name = "Billing" } }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        var result = await _sut.GetAllAsync(2534);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var returnedList = objectResult.Value as List<BusinessEntityAddressModel>;
            returnedList.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetAllAsync_returns_200_with_empty_listAsync()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessEntityAddressModel>());

        var result = await _sut.GetAllAsync(9999);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var returnedList = objectResult.Value as List<BusinessEntityAddressModel>;
            returnedList.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetAllAsync_sends_correct_storeId_to_mediatorAsync()
    {
        const int storeId = 2534;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessEntityAddressModel>());

        await _sut.GetAllAsync(storeId);

        _mockMediator.Verify(x => x.Send(
            It.Is<ReadStoreAddressListQuery>(q => q.StoreId == storeId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
