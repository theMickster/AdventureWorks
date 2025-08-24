using AdventureWorks.Models.Features.ProductReview;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Queries;

public sealed class ReadProductReviewStatisticsQuery : IRequest<ProductReviewStatisticsModel>
{
    public required int ProductId { get; set; }
}
