using AdventureWorks.Models.Features.AddressManagement;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadCountryRegionListQuery : IRequest<List<CountryRegionModel>>
{
}
