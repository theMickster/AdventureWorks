using AdventureWorks.Application.PersistenceContracts.Repositories;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Commands;

/// <summary>
/// Handles the deletion of an existing product review.
/// </summary>
public sealed class DeleteProductReviewCommandHandler(
    IProductReviewRepository productReviewRepository)
        : IRequestHandler<DeleteProductReviewCommand>
{
    private readonly IProductReviewRepository _productReviewRepository = productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));

    /// <summary>
    /// Handles the command to delete an existing product review.
    /// </summary>
    /// <param name="request">the command request</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task Handle(DeleteProductReviewCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingEntity = await _productReviewRepository.GetByIdAsync(request.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Product review with ID {request.Id} not found.");
        await _productReviewRepository.DeleteAsync(existingEntity, cancellationToken);
    }
}
