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
/// Handler for <see cref="AddStoreAddressCommand"/>.
/// Validates the payload (Rule-01 AddressTypeId, Rule-02 AddressId), enforces composite-key uniqueness
/// (Rule-03), and persists the new address.
/// </summary>
public sealed class AddStoreAddressCommandHandler(
    IMapper mapper,
    IBusinessEntityAddressRepository businessEntityAddressRepository,
    IStoreRepository storeRepository,
    IValidator<StoreAddressCreateModel> validator)
        : IRequestHandler<AddStoreAddressCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IBusinessEntityAddressRepository _businessEntityAddressRepository = businessEntityAddressRepository ?? throw new ArgumentNullException(nameof(businessEntityAddressRepository));
    private readonly IStoreRepository _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    private readonly IValidator<StoreAddressCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(AddStoreAddressCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        if (!await _storeRepository.ExistsAsync(request.StoreId, cancellationToken))
        {
            throw new KeyNotFoundException($"Store with ID {request.StoreId} not found.");
        }

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        if (await _businessEntityAddressRepository.ExistsAsync(request.StoreId, request.Model.AddressId, request.Model.AddressTypeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(StoreAddressCreateModel.AddressTypeId), MessageDuplicateAddress)
                {
                    ErrorCode = "Rule-03"
                }
            });
        }

        var entity = _mapper.Map<BusinessEntityAddressEntity>(request.Model);
        entity.BusinessEntityId = request.StoreId;
        entity.ModifiedDate = request.ModifiedDate;
        entity.Rowguid = request.RowGuid;

        await _businessEntityAddressRepository.AddAsync(entity, cancellationToken);

        return request.Model.AddressId;
    }

    public static string MessageDuplicateAddress => "An address with the same address and address type already exists for this store.";
}
