using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Constants;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="ReassignStoreSalesPersonCommand"/>.
/// Validates the payload (Rule-01 SalesPersonId), enforces business rules (Rule-02 already assigned,
/// Rule-03 sales person does not exist), closes the open history row, inserts a new open row,
/// and updates Store.SalesPersonId. Returns the StoreId on success.
/// </summary>
public sealed class ReassignStoreSalesPersonCommandHandler(
    IStoreRepository storeRepository,
    ISalesPersonRepository salesPersonRepository,
    IValidator<StoreSalesPersonAssignmentCreateModel> validator)
        : IRequestHandler<ReassignStoreSalesPersonCommand, int>
{
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly ISalesPersonRepository _salesPersonRepository = salesPersonRepository ?? throw new ArgumentNullException(nameof(salesPersonRepository));
    private readonly IValidator<StoreSalesPersonAssignmentCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(ReassignStoreSalesPersonCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var store = await _storeRepository.GetStoreByIdAsync(request.StoreId, includeAddresses: false, cancellationToken)
            ?? throw new KeyNotFoundException($"Store with ID {request.StoreId} was not found.");

        if (request.Model.SalesPersonId == store.SalesPersonId)
        {
            throw SamePersonException();
        }

        if (!await _salesPersonRepository.ExistsAsync(request.Model.SalesPersonId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(StoreSalesPersonAssignmentCreateModel.SalesPersonId), MessageSalesPersonNotFound)
                {
                    ErrorCode = "Rule-03"
                }
            });
        }

        try
        {
            await _storeRepository.ReassignSalesPersonAsync(request.StoreId, request.Model.SalesPersonId, request.AssignedDate, cancellationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message == SalesConstants.SameSalesPersonSentinel)
        {
            throw SamePersonException();
        }

        return request.StoreId;
    }

    private static ValidationException SamePersonException() =>
        new(new[]
        {
            new ValidationFailure(nameof(StoreSalesPersonAssignmentCreateModel.SalesPersonId), MessageAlreadyAssigned)
            {
                ErrorCode = "Rule-02"
            }
        });

    public static string MessageAlreadyAssigned => "The store is already assigned to this sales person.";

    public static string MessageSalesPersonNotFound => "The specified sales person does not exist.";
}
