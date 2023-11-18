using System.Collections;
using AdventureWorks.API.Controllers.v1.CountryRegion;
using AdventureWorks.Application.Interfaces.Services.CountryRegion;
using AdventureWorks.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.CountryRegion;

[ExcludeFromCodeCoverage]
public sealed class ReadCountryRegionControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadCountryRegionController>> _mockLogger = new();
    private readonly Mock<IReadCountryRegionService> _mockReadCountryRegionService = new();
    private readonly ReadCountryRegionController _sut;

    public ReadCountryRegionControllerTests()
    {
        _sut = new ReadCountryRegionController(_mockLogger.Object, _mockReadCountryRegionService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadCountryRegionController(null!, _mockReadCountryRegionService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadCountryRegionController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readCountryRegionService");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockReadCountryRegionService.Setup(
                x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync( new CountryRegionModel {Code = "JP", Name = "Japan"});

        var result = await _sut.GetByIdAsync("JP");
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
        _mockReadCountryRegionService.Setup(
                x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((CountryRegionModel)null!);

        var result = await _sut.GetByIdAsync("JP");
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the country region.");
        }
    }

    [Fact]
    public async Task GetById_returns_bad_request_Async()
    {
        var result = await _sut.GetByIdAsync("    ");
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid country region id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockReadCountryRegionService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(
                new List<CountryRegionModel>
                {
                    new() { Code = "JP", Name = "Japan" }, new() { Code = "KO", Name = "South Korea" }
                });

        var result = await _sut.GetListAsync();

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
        _mockReadCountryRegionService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(new List<CountryRegionModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records the country region list.");
        }
    }
}