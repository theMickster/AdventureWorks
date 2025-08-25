using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for <see cref="ReadStoreContactQuery"/>.
/// Retrieves a single store contact (with ContactType and Person details) by its composite key
/// and maps it to <see cref="StoreContactModel"/>. Returns <c>null</c> when the contact does not exist.
/// </summary>
public sealed class ReadStoreContactQueryHandler(
    IMapper mapper,
    IBusinessEntityContactEntityRepository businessEntityContactRepository)
        : IRequestHandler<ReadStoreContactQuery, StoreContactModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IBusinessEntityContactEntityRepository _businessEntityContactRepository = businessEntityContactRepository ?? throw new ArgumentNullException(nameof(businessEntityContactRepository));

    public async Task<StoreContactModel?> Handle(ReadStoreContactQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _businessEntityContactRepository.GetWithDetailsByCompositeKeyAsync(
            request.StoreId, request.PersonId, request.ContactTypeId, cancellationToken);

        return entity is null ? null : _mapper.Map<StoreContactModel>(entity);
    }
}
