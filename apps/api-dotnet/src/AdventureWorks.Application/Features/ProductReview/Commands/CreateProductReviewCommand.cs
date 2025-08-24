using AdventureWorks.Models.Features.ProductReview;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Commands;

public sealed class CreateProductReviewCommand : IRequest<int>
{
    public required ProductReviewCreateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }
}
