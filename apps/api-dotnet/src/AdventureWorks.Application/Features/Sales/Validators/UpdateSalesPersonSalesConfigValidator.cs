using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;

namespace AdventureWorks.Application.Features.Sales.Validators;

/// <summary>
/// Validates a <see cref="SalesPersonSalesConfigUpdateModel"/>, adding Rule-06 (Id must be positive) and Rule-07 (territory must exist) on top of the base rules.
/// </summary>
public sealed class UpdateSalesPersonSalesConfigValidator : SalesPersonBaseModelValidator<SalesPersonSalesConfigUpdateModel>
{
    private readonly ISalesTerritoryRepository _salesTerritoryRepository;

    public UpdateSalesPersonSalesConfigValidator(ISalesTerritoryRepository salesTerritoryRepository)
    {
        _salesTerritoryRepository = salesTerritoryRepository ?? throw new ArgumentNullException(nameof(salesTerritoryRepository));

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithErrorCode("Rule-06")
            .WithMessage(MessageIdRequired);

        RuleFor(x => x.TerritoryId)
            .MustAsync(async (id, cancellation) => await TerritoryMustExistAsync(id!.Value, cancellation))
            .When(x => x.TerritoryId.HasValue && x.TerritoryId > 0)
            .WithErrorCode("Rule-07")
            .WithMessage(MessageTerritoryIdMustExist);
    }

    public static string MessageIdRequired => "Sales Person ID must be greater than 0";
    public static string MessageTerritoryIdMustExist => "Territory ID must reference an existing sales territory";

    private async Task<bool> TerritoryMustExistAsync(int territoryId, CancellationToken cancellationToken)
    {
        var result = await _salesTerritoryRepository.GetByIdAsync(territoryId, cancellationToken).ConfigureAwait(false);
        return result != null;
    }
}
