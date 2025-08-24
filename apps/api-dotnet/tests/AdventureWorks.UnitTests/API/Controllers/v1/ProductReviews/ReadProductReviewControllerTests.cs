using AdventureWorks.API.Controllers.v1.ProductReviews;
using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.ProductReview;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ProductReviews;

[ExcludeFromCodeCoverage]
public sealed class ReadProductReviewControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductReviewController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductReviewController _sut;

    public ReadProductReviewControllerTests()
    {
        _sut = new ReadProductReviewController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductReviewController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductReviewController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetProductReviewListAsync_returns_ok_with_results_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductReviewListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductReviewSearchResultModel
            {
                Results = [new() { ProductReviewId = 1, ProductId = 937, ReviewerName = "Alice", Rating = 5 }]
            });

        var result = await _sut.GetProductReviewListAsync(937, new ProductReviewParameter());
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetProductReviewListAsync_returns_ok_with_empty_results_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductReviewListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductReviewSearchResultModel
            {
                Results = new List<ProductReviewModel>()
            });

        var result = await _sut.GetProductReviewListAsync(937, new ProductReviewParameter());
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetProductReviewListAsync_returns_bad_request_for_invalid_productId_Async(int productId)
    {
        var result = await _sut.GetProductReviewListAsync(productId, new ProductReviewParameter());
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid product id must be specified.");
        }
    }
}
