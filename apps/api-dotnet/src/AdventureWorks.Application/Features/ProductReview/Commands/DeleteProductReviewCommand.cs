using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Commands;

public sealed class DeleteProductReviewCommand : IRequest
{
    public int Id { get; set; }
}
