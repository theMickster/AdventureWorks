using AdventureWorks.API.Controllers.v1.Address;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Application.Features.AddressManagement.Contracts;

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
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadAddressController(null!, _mockReadAddressService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readAddressService");
        }
    }

    [Fact]
    public async Task getById_returns_ok_Async()
    {
        const int id = 7;

        _mockReadAddressService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new AddressModel{Id = id} );

        var result = await _sut.GetByIdAsync(7);

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

        var result = await _sut.GetByIdAsync(7);

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
        var result = await _sut.GetByIdAsync(addressId);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}
