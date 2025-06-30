using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

public sealed class ReadPhoneNumberTypeListQueryHandler(
    IMapper mapper,
    IPhoneNumberTypeRepository phoneNumberTypeRepository)
    : IRequestHandler<ReadPhoneNumberTypeListQuery, List<PhoneNumberTypeModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPhoneNumberTypeRepository _repository = phoneNumberTypeRepository ?? throw new ArgumentNullException(nameof(phoneNumberTypeRepository));

    public async Task<List<PhoneNumberTypeModel>> Handle(ReadPhoneNumberTypeListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAllAsync();
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<PhoneNumberTypeModel>>(entities);
    }
}
