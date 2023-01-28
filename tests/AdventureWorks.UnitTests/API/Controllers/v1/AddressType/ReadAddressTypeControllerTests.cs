using AdventureWorks.API.Controllers.v1.AddressType;
using AdventureWorks.Application.Interfaces.Services.AddressType;
using AdventureWorks.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.AddressType;

[ExcludeFromCodeCoverage]
public sealed class ReadAddressTypeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadAddressTypeController>> _mockLogger = new();
    private readonly Mock<IReadAddressTypeService> _mockReadAddressTypeService = new();
    private readonly ReadAddressTypeController _sut;

    public ReadAddressTypeControllerTests()
    {
        _sut = new ReadAddressTypeController(_mockLogger.Object, _mockReadAddressTypeService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadAddressTypeController(null!, _mockReadAddressTypeService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadAddressTypeController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readAddressTypeService");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockReadAddressTypeService.Setup(
                x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new AddressTypeModel { Id = 1, Name = "Home"});

        var result = await _sut.GetByIdAsync(123).ConfigureAwait(false);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockReadAddressTypeService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((AddressTypeModel)null!);

        var result = await _sut.GetByIdAsync(123456).ConfigureAwait(false);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the address type.");
        }
    }

    [Fact]
    public async Task GetById_returns_bad_request_Async()
    {
        var result = await _sut.GetByIdAsync(-100).ConfigureAwait(false);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid address type id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockReadAddressTypeService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(
                new List<AddressTypeModel>
                {
                    new() { Id = 1, Name = "Home"}
                    ,new() { Id = 2, Name = "Billing"}
                    ,new() { Id = 3, Name = "Mailing"}
                });

        var result = await _sut.GetListAsync().ConfigureAwait(false);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetList_returns_not_found_Async()
    {
        _mockReadAddressTypeService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(new List<AddressTypeModel>());

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records the address type list.");
        }
    }
}
