using AdventureWorks.API.Controllers.v1.Address;
using AdventureWorks.Application.Interfaces.Services.Address;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using AdventureWorks.Domain.Models;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Address;

[ExcludeFromCodeCoverage]
public sealed class ReadAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadAddressController>> _mockLogger = new();
    private readonly Mock<IReadAddressService> _mockReadAddressService = new();
    private readonly ReadAddressController _sut;

    public ReadAddressControllerTests()
    {
        _sut = new ReadAddressController(_mockLogger.Object, _mockReadAddressService.Object);
    }

    [Fact]
    public async Task getById_returns_ok_Async()
    {
        const int id = 7;

        _mockReadAddressService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new AddressModel{Id = id} );

        var result = await _sut.GetByIdAsync(7).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task getById_returns_not_found_Async()
    {
        _mockReadAddressService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((AddressModel?)null);

        var result = await _sut.GetByIdAsync(7).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task getById_returns_bad_request_Async(int addressId)
    {
        var result = await _sut.GetByIdAsync(addressId).ConfigureAwait(false);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}
