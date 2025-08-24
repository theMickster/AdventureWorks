using AdventureWorks.Models.Features.ProductReview;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Queries;

/// <summary>
/// Query to retrieve a single product review by its unique identifier.
/// </summary>
public sealed class ReadProductReviewQuery : IRequest<ProductReviewModel?>
{
    public required int Id { get; set; }
}
