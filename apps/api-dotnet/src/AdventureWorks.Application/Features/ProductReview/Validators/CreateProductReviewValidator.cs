using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.ProductReview;
using FluentValidation;

namespace AdventureWorks.Application.Features.ProductReview.Validators;

public sealed class CreateProductReviewValidator : ProductReviewBaseModelValidator<ProductReviewCreateModel>
{
    public CreateProductReviewValidator(IProductRepository productRepository)
    {
        ArgumentNullException.ThrowIfNull(productRepository);

        RuleFor(x => x.ProductId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithErrorCode("Rule-08").WithMessage(MessageProductIdInvalid)
            .MustAsync(async (id, ct) => await productRepository.ExistsAsync(id, ct))
            .WithErrorCode("Rule-09").WithMessage(MessageProductIdNotFound);
    }

    public static string MessageProductIdInvalid => "Product ID must be greater than 0.";
    public static string MessageProductIdNotFound => "The specified product does not exist.";
}
