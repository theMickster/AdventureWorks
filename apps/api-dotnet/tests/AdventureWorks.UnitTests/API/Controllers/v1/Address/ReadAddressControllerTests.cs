using AdventureWorks.API.Controllers.v1.Address;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Address;

[ExcludeFromCodeCoverage]
public sealed class ReadAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadAddressController _sut;

    public ReadAddressControllerTests()
    {
        _sut = new ReadAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task getById_returns_ok_Async()
    {
        const int id = 7;

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadAddressQuery>(), It.IsAny<CancellationToken>()))
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
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadAddressQuery>(), It.IsAny<CancellationToken>()))!
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
