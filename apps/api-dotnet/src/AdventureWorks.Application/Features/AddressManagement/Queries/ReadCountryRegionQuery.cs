using AdventureWorks.Models.Features.AddressManagement;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadCountryRegionQuery : IRequest<CountryRegionModel>
{
    public required string Code { get; set; } = string.Empty;
}
