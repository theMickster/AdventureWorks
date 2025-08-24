using AdventureWorks.API.Controllers.v1.ProductReviews;
using AdventureWorks.Application.Features.ProductReview.Commands;
using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Models.Features.ProductReview;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ProductReviews;

[ExcludeFromCodeCoverage]
public sealed class UpdateProductReviewControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<UpdateProductReviewController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly UpdateProductReviewController _sut;

    public UpdateProductReviewControllerTests()
    {
        _sut = new UpdateProductReviewController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateProductReviewController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new UpdateProductReviewController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateProductReviewAsync_returns_bad_request_when_review_id_is_invalid_Async(int reviewId)
    {
        var model = new ProductReviewUpdateModel
        {
            ReviewerName = "Alice",
            EmailAddress = "alice@example.com",
            Rating = 5
        };

        var result = await _sut.UpdateProductReviewAsync(reviewId, model);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task UpdateProductReviewAsync_returns_bad_request_when_model_is_null_Async()
    {
        var result = await _sut.UpdateProductReviewAsync(1, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            (objectResult.Value as string).Should().Be("The product review input model cannot be null.");
        }
    }

    [Fact]
    public async Task UpdateProductReviewAsync_returns_ok_with_updated_model_on_success_Async()
    {
        const int reviewId = 7;

        var model = new ProductReviewUpdateModel
        {
            ReviewerName = "Alice",
            EmailAddress = "alice@example.com",
            Rating = 5,
            Comments = "Updated comment"
        };

        var expectedModel = new ProductReviewModel
        {
            ProductReviewId = reviewId,
            ProductId = 937,
            ReviewerName = "Alice",
            EmailAddress = "alice@example.com",
            Rating = 5,
            Comments = "Updated comment"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateProductReviewCommand>(), It.IsAny<CancellationToken>()));

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadProductReviewQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModel);

        var result = await _sut.UpdateProductReviewAsync(reviewId, model);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            okResult.Value.Should().BeEquivalentTo(expectedModel);
        }
    }

    [Fact]
    public async Task UpdateProductReviewAsync_propagates_key_not_found_exception_from_command_Async()
    {
        const int reviewId = 999;

        var model = new ProductReviewUpdateModel
        {
            ReviewerName = "Alice",
            EmailAddress = "alice@example.com",
            Rating = 5
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<UpdateProductReviewCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Product review with ID {reviewId} not found."));

        Func<Task> act = async () => await _sut.UpdateProductReviewAsync(reviewId, model);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{reviewId}*");
    }
}
