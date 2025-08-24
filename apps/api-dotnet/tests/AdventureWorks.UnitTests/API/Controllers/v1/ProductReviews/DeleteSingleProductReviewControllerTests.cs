using AdventureWorks.API.Controllers.v1.ProductReviews;
using AdventureWorks.Application.Features.ProductReview.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ProductReviews;

[ExcludeFromCodeCoverage]
public sealed class DeleteSingleProductReviewControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<DeleteSingleProductReviewController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly DeleteSingleProductReviewController _sut;

    public DeleteSingleProductReviewControllerTests()
    {
        _sut = new DeleteSingleProductReviewController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new DeleteSingleProductReviewController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new DeleteSingleProductReviewController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteProductReviewAsync_returns_bad_request_when_review_id_is_invalid_Async(int reviewId)
    {
        var result = await _sut.DeleteProductReviewAsync(reviewId);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task DeleteProductReviewAsync_returns_no_content_on_success_Async()
    {
        const int reviewId = 7;

        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteProductReviewCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        var result = await _sut.DeleteProductReviewAsync(reviewId);

        var noContentResult = result as NoContentResult;

        using (new AssertionScope())
        {
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }
    }

    [Fact]
    public async Task DeleteProductReviewAsync_propagates_key_not_found_exception_Async()
    {
        const int reviewId = 999;

        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteProductReviewCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Product review with ID {reviewId} not found."));

        Func<Task> act = async () => await _sut.DeleteProductReviewAsync(reviewId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{reviewId}*");
    }
}
