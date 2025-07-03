using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

public sealed class ReadPhoneNumberTypeQueryHandler(
    IMapper mapper,
    IPhoneNumberTypeRepository phoneNumberTypeRepository)
        : IRequestHandler<ReadPhoneNumberTypeQuery, PhoneNumberTypeModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPhoneNumberTypeRepository _repository = phoneNumberTypeRepository ?? throw new ArgumentNullException(nameof(phoneNumberTypeRepository));

    public async Task<PhoneNumberTypeModel> Handle(ReadPhoneNumberTypeQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<PhoneNumberTypeModel>(await _repository.GetByIdAsync(request.Id));
    }
}
