using AdventureWorks.Models.Features.ProductReview;
using FluentValidation;

namespace AdventureWorks.Application.Features.ProductReview.Validators;

public sealed class UpdateProductReviewValidator : ProductReviewBaseModelValidator<ProductReviewUpdateModel>
{
    public UpdateProductReviewValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithErrorCode("Rule-08").WithMessage(MessageIdInvalid);
    }

    public static string MessageIdInvalid => "A valid product review ID must be specified.";
}
