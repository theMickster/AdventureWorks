using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="AddStoreContactCommand"/>.
/// Validates the payload (Rule-01 PersonId, Rule-02 ContactTypeId), enforces composite-key uniqueness
/// (Rule-03), and persists the new contact.
/// </summary>
public sealed class AddStoreContactCommandHandler(
    IMapper mapper,
    IBusinessEntityContactEntityRepository businessEntityContactRepository,
    IStoreRepository storeRepository,
    IValidator<StoreContactCreateModel> validator)
        : IRequestHandler<AddStoreContactCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IBusinessEntityContactEntityRepository _businessEntityContactRepository = businessEntityContactRepository ?? throw new ArgumentNullException(nameof(businessEntityContactRepository));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IValidator<StoreContactCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(AddStoreContactCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        if (!await _storeRepository.ExistsAsync(request.StoreId, cancellationToken))
        {
            throw new KeyNotFoundException($"Store with ID {request.StoreId} not found.");
        }

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        if (await _businessEntityContactRepository.ExistsAsync(request.StoreId, request.Model.PersonId, request.Model.ContactTypeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(StoreContactCreateModel.ContactTypeId), MessageDuplicateContact)
                {
                    ErrorCode = "Rule-03"
                }
            });
        }

        var entity = _mapper.Map<BusinessEntityContactEntity>(request.Model);
        entity.BusinessEntityId = request.StoreId;
        entity.ModifiedDate = request.ModifiedDate;
        entity.Rowguid = request.RowGuid;

        await _businessEntityContactRepository.AddAsync(entity, cancellationToken);

        return request.Model.PersonId;
    }

    public static string MessageDuplicateContact => "A contact with the same person and contact type already exists for this store.";
}
