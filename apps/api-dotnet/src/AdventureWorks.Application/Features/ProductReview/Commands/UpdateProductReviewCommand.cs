using AdventureWorks.Models.Features.ProductReview;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Commands;

public sealed class UpdateProductReviewCommand : IRequest
{
    public required ProductReviewUpdateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }
}
