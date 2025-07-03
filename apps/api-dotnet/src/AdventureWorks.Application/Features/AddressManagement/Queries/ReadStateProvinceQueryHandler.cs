using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadStateProvinceQueryHandler (
    IMapper mapper,
    IStateProvinceRepository repository) 
        : IRequestHandler<ReadStateProvinceQuery, StateProvinceModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IStateProvinceRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<StateProvinceModel> Handle(ReadStateProvinceQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<StateProvinceModel>(await _repository.GetByIdAsync(request.Id));
    }
}
