using AdventureWorks.API.Controllers.v1.SalesTerritory;
using AdventureWorks.Application.Interfaces.Services.SalesTerritory;
using AdventureWorks.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesTerritory;

[ExcludeFromCodeCoverage]
public sealed class ReadSalesTerritoryControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadSalesTerritoryController>> _mockLogger = new();
    private readonly Mock<IReadSalesTerritoryService> _mockReadSalesTerritoryService = new();
    private readonly ReadSalesTerritoryController _sut;

    public ReadSalesTerritoryControllerTests()
    {
        _sut = new ReadSalesTerritoryController(_mockLogger.Object, _mockReadSalesTerritoryService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadSalesTerritoryController(null!, _mockReadSalesTerritoryService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadSalesTerritoryController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readSalesTerritoryService");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockReadSalesTerritoryService.Setup(
                x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SalesTerritoryModel { Id = 1, Name = "Home" });

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
        _mockReadSalesTerritoryService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SalesTerritoryModel)null!);

        var result = await _sut.GetByIdAsync(123456).ConfigureAwait(false);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the sales territory.");
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
            outputModel!.Should().Be("A valid sales territory id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockReadSalesTerritoryService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(
                new List<SalesTerritoryModel>
                {
                    new() { Id = 1, Name = "West"}
                    ,new() { Id = 2, Name = "Central"}
                    ,new() { Id = 3, Name = "East"}
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
        _mockReadSalesTerritoryService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(new List<SalesTerritoryModel>());

        var result = await _sut.GetListAsync().ConfigureAwait(false);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records the sales territory list.");
        }
    }
}
