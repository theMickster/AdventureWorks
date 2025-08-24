using AdventureWorks.Models.Features.ProductReview;
using FluentValidation;

namespace AdventureWorks.Application.Features.ProductReview.Validators;

public class ProductReviewBaseModelValidator<T> : AbstractValidator<T> where T : ProductReviewBaseModel
{
    public ProductReviewBaseModelValidator()
    {
        RuleFor(x => x.ReviewerName)
            .NotEmpty().WithErrorCode("Rule-01").WithMessage(MessageReviewerNameEmpty)
            .MaximumLength(50).WithErrorCode("Rule-02").WithMessage(MessageReviewerNameLength);

        RuleFor(x => x.EmailAddress)
            .NotEmpty().WithErrorCode("Rule-03").WithMessage(MessageEmailAddressEmpty)
            .EmailAddress().WithErrorCode("Rule-04").WithMessage(MessageEmailAddressInvalid)
            .MaximumLength(50).WithErrorCode("Rule-05").WithMessage(MessageEmailAddressLength);

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithErrorCode("Rule-06").WithMessage(MessageRatingRange);

        RuleFor(x => x.Comments)
            .MaximumLength(3850).When(x => x.Comments != null)
            .WithErrorCode("Rule-07").WithMessage(MessageCommentsLength);
    }

    public static string MessageReviewerNameEmpty => "Reviewer name cannot be null, empty, or whitespace.";
    public static string MessageReviewerNameLength => "Reviewer name cannot exceed 50 characters.";
    public static string MessageEmailAddressEmpty => "Email address cannot be null, empty, or whitespace.";
    public static string MessageEmailAddressInvalid => "Email address must be a valid email format.";
    public static string MessageEmailAddressLength => "Email address cannot exceed 50 characters.";
    public static string MessageRatingRange => "Rating must be between 1 and 5.";
    public static string MessageCommentsLength => "Comments cannot exceed 3850 characters.";
}
