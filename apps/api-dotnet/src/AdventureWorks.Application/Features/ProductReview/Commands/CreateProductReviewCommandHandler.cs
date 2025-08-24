using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.ProductReview.Commands;

/// <summary>
/// Handles the creation of a new product review.
/// </summary>
public sealed class CreateProductReviewCommandHandler(
    IMapper mapper,
    IProductReviewRepository productReviewRepository,
    IValidator<ProductReviewCreateModel> validator)
        : IRequestHandler<CreateProductReviewCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IProductReviewRepository _productReviewRepository = productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));
    private readonly IValidator<ProductReviewCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Handles the command to create a new product review.
    /// </summary>
    /// <param name="request">the command request</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    public async Task<int> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var inputEntity = _mapper.Map<Domain.Entities.Production.ProductReview>(request.Model);
        inputEntity.ReviewDate = request.ReviewDate;
        inputEntity.ModifiedDate = request.ModifiedDate;

        var outputEntity = await _productReviewRepository.AddAsync(inputEntity, cancellationToken);

        return outputEntity.ProductReviewId;
    }
}
