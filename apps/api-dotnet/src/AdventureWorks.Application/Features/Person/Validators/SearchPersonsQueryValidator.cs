using AdventureWorks.Application.Features.Person.Queries;
using FluentValidation;

namespace AdventureWorks.Application.Features.Person.Validators;

/// <summary>
/// Validates the SearchPersonsQuery payload.
/// Rule-01: At least one filter is required.
/// Rule-02: Page must be >= 1.
/// Rule-03: PageSize must be between 1 and 100.
/// </summary>
public sealed class SearchPersonsQueryValidator : AbstractValidator<SearchPersonsQuery>
{
    public SearchPersonsQueryValidator()
    {
        RuleFor(x => x)
            .Custom((query, context) =>
            {
                if (string.IsNullOrWhiteSpace(query.FirstName) &&
                    string.IsNullOrWhiteSpace(query.LastName) &&
                    string.IsNullOrWhiteSpace(query.PersonTypeCode))
                {
                    context.AddFailure(
                        new FluentValidation.Results.ValidationFailure(
                            "SearchFilters",
                            MessageAtLeastOneFilterRequired)
                        {
                            ErrorCode = "Rule-01"
                        });
                }
            });

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode("Rule-02")
            .WithMessage(MessagePageMustBeGreaterThanOrEqualToOne);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithErrorCode("Rule-03")
            .WithMessage(MessagePageSizeMustBeBetween1And100);
    }

    public static string MessageAtLeastOneFilterRequired =>
        "At least one search criterion (firstName, lastName, or personTypeCode) is required.";

    public static string MessagePageMustBeGreaterThanOrEqualToOne =>
        "Page must be greater than or equal to 1.";

    public static string MessagePageSizeMustBeBetween1And100 =>
        "PageSize must be between 1 and 100.";
}
