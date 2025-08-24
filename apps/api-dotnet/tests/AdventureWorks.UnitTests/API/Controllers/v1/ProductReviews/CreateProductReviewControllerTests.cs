using AdventureWorks.API.Controllers.v1.ProductReviews;
using AdventureWorks.Application.Features.ProductReview.Commands;
using AdventureWorks.Models.Features.ProductReview;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ProductReviews;

[ExcludeFromCodeCoverage]
public sealed class CreateProductReviewControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateProductReviewController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateProductReviewController _sut;

    public CreateProductReviewControllerTests()
    {
        _sut = new CreateProductReviewController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateProductReviewController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateProductReviewController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task CreateProductReviewAsync_returns_bad_request_when_model_is_null_Async()
    {
        var result = await _sut.CreateProductReviewAsync(null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            (objectResult.Value as string).Should().Be("The product review input model cannot be null.");
        }
    }

    [Fact]
    public async Task CreateProductReviewAsync_returns_created_with_correct_route_on_success_Async()
    {
        const int newReviewId = 7;

        var model = new ProductReviewCreateModel
        {
            ProductId = 937,
            ReviewerName = "Alice",
            EmailAddress = "alice@example.com",
            Rating = 5,
            Comments = "Great product!"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<CreateProductReviewCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newReviewId);

        var result = await _sut.CreateProductReviewAsync(model);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult.RouteName.Should().Be("ReadSingleProductReviewControllerV1");

            var routeValues = createdResult.RouteValues;
            routeValues.Should().ContainKey("reviewId");
            routeValues!["reviewId"].Should().Be(newReviewId);
        }
    }

    [Fact]
    public async Task CreateProductReviewAsync_propagates_mediator_exceptions_Async()
    {
        var model = new ProductReviewCreateModel
        {
            ProductId = 937,
            ReviewerName = "Alice",
            EmailAddress = "alice@example.com",
            Rating = 5
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<CreateProductReviewCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "Rating", ErrorCode = "Rule-06", ErrorMessage = "Rating must be between 1 and 5." } }));

        Func<Task> act = async () => await _sut.CreateProductReviewAsync(model);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
