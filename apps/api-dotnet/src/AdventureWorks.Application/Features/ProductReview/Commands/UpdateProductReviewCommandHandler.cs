using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Commands;

/// <summary>
/// Handles the update of an existing product review.
/// </summary>
public sealed class UpdateProductReviewCommandHandler(
    IMapper mapper,
    IProductReviewRepository productReviewRepository,
    IValidator<ProductReviewUpdateModel> validator)
        : IRequestHandler<UpdateProductReviewCommand>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductReviewRepository _productReviewRepository = productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));
    private readonly IValidator<ProductReviewUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Handles the command to update an existing product review.
    /// </summary>
    /// <param name="request">the command request</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task Handle(UpdateProductReviewCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var existingEntity = await _productReviewRepository.GetByIdAsync(request.Model.Id, cancellationToken);
        if (existingEntity is null)
        {
            throw new KeyNotFoundException($"Product review with ID {request.Model.Id} not found.");
        }

        _mapper.Map(request.Model, existingEntity);
        existingEntity.ModifiedDate = request.ModifiedDate;

        await _productReviewRepository.UpdateAsync(existingEntity, cancellationToken);
    }
}
