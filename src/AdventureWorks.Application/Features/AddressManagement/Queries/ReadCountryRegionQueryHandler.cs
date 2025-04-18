using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadCountryRegionQueryHandler(
    IMapper mapper, 
    ICountryRegionRepository repository)
        : IRequestHandler<ReadCountryRegionQuery, CountryRegionModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ICountryRegionRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<CountryRegionModel> Handle(ReadCountryRegionQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<CountryRegionModel>(await _repository.GetByIdAsync(request.Code));
    }
}