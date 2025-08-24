using AdventureWorks.API.Controllers.v1.ProductReviews;
using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Models.Features.ProductReview;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ProductReviews;

[ExcludeFromCodeCoverage]
public sealed class ReadSingleProductReviewControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadSingleProductReviewController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadSingleProductReviewController _sut;

    public ReadSingleProductReviewControllerTests()
    {
        _sut = new ReadSingleProductReviewController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadSingleProductReviewController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadSingleProductReviewController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetProductReviewAsync_returns_ok_with_model_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductReviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductReviewModel { ProductReviewId = 1, ProductId = 937, ReviewerName = "Alice", Rating = 5 });

        var result = await _sut.GetProductReviewAsync(1);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<ProductReviewModel>();
        }
    }

    [Fact]
    public async Task GetProductReviewAsync_returns_not_found_when_handler_returns_null_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductReviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductReviewModel?)null);

        var result = await _sut.GetProductReviewAsync(999);
        var objectResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            (objectResult.Value as string).Should().Be("Unable to locate the product review.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetProductReviewAsync_returns_bad_request_for_invalid_reviewId_Async(int reviewId)
    {
        var result = await _sut.GetProductReviewAsync(reviewId);
        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            (objectResult.Value as string).Should().Be("A valid product review id must be specified.");
        }
    }
}
