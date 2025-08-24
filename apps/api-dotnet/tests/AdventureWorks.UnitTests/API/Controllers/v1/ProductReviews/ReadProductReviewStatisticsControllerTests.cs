using AdventureWorks.API.Controllers.v1.ProductReviews;
using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Models.Features.ProductReview;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ProductReviews;

[ExcludeFromCodeCoverage]
public sealed class ReadProductReviewStatisticsControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadProductReviewStatisticsController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadProductReviewStatisticsController _sut;

    public ReadProductReviewStatisticsControllerTests()
    {
        _sut = new ReadProductReviewStatisticsController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadProductReviewStatisticsController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadProductReviewStatisticsController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetProductReviewStatisticsAsync_returns_bad_request_for_invalid_productId_Async(int productId)
    {
        var result = await _sut.GetProductReviewStatisticsAsync(productId);
        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            (objectResult.Value as string).Should().Be("A valid product id must be specified.");
        }
    }

    [Fact]
    public async Task GetProductReviewStatisticsAsync_returns_ok_with_statistics_Async()
    {
        var expected = new ProductReviewStatisticsModel
        {
            ProductId = 937,
            TotalReviews = 5,
            AverageRating = 3.8,
            RatingDistribution = new Dictionary<int, int> { { 1, 1 }, { 2, 0 }, { 3, 1 }, { 4, 0 }, { 5, 3 } }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductReviewStatisticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetProductReviewStatisticsAsync(937);
        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public async Task GetProductReviewStatisticsAsync_returns_ok_with_zero_stats_when_no_reviews_Async()
    {
        var expected = new ProductReviewStatisticsModel
        {
            ProductId = 99999,
            TotalReviews = 0,
            AverageRating = 0.0,
            RatingDistribution = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } }
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductReviewStatisticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetProductReviewStatisticsAsync(99999);
        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().BeEquivalentTo(expected);
        }
    }
}
