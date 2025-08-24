using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.ProductReview;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Queries;

/// <summary>
/// Query to retrieve a paginated list of product reviews for a given product.
/// </summary>
public sealed class ReadProductReviewListQuery : IRequest<ProductReviewSearchResultModel>
{
    public required int ProductId { get; set; }

    public required ProductReviewParameter Parameters { get; set; }
}
