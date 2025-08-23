using AdventureWorks.API.Controllers.v1.Products;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Production;
using AdventureWorks.Test.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Products;

[ExcludeFromCodeCoverage]
public sealed class ReadProductListControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductListController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductListController _sut;

    public ReadProductListControllerTests()
    {
        _sut = new ReadProductListController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductListController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductListController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetProductListAsync_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductSearchResultModel { Results = [new() { Id = 1, Name = "Mountain Bike" }] });

        var result = await _sut.GetProductListAsync(new ProductParameter());
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetProductListAsync_null_results_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductSearchResultModel { Results = null! });

        var result = await _sut.GetProductListAsync(new ProductParameter());
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            _mockLogger.VerifyLoggingMessageContains("No results found for product list", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task GetProductListAsync_empty_results_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductSearchResultModel { Results = new List<ProductListModel>() });

        var result = await _sut.GetProductListAsync(new ProductParameter());
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            _mockLogger.VerifyLoggingMessageContains("No results found for product list", null, LogLevel.Information);
        }
    }
}
