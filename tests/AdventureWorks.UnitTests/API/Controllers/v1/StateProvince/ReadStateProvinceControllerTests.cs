using AdventureWorks.API.Controllers.v1.StateProvince;
using AdventureWorks.Application.Interfaces.Services.StateProvince;
using AdventureWorks.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.StateProvince;

[ExcludeFromCodeCoverage]
public sealed class ReadStateProvinceControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadStateProvinceController>> _mockLogger = new();
    private readonly Mock<IReadStateProvinceService> _mockReadStateProvinceService = new();
    private readonly ReadStateProvinceController _sut;

    public ReadStateProvinceControllerTests()
    {
        _sut = new ReadStateProvinceController(_mockLogger.Object, _mockReadStateProvinceService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadStateProvinceController(null!, _mockReadStateProvinceService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadStateProvinceController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readStateProvinceService");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockReadStateProvinceService.Setup(
                x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new StateProvinceModel
                { Code = "JP", Name = "Japan", Id = 1, IsStateProvinceCodeUnavailable = true });

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
        _mockReadStateProvinceService.Setup(
                x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((StateProvinceModel)null!);

        var result = await _sut.GetByIdAsync(123456).ConfigureAwait(false);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the state province.");
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
            outputModel!.Should().Be("A valid state province id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockReadStateProvinceService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(
                new List<StateProvinceModel>
                {
                    new() { Code = "JP", Name = "Japan", Id = 1, IsStateProvinceCodeUnavailable = true}
                    ,new() { Code = "KO", Name = "South Korea", Id = 2, IsStateProvinceCodeUnavailable = false}
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
        _mockReadStateProvinceService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(new List<StateProvinceModel>());

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records the state province list.");
        }
    }
}
