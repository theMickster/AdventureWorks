using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadLocationListQuery : IRequest<List<LocationModel>>
{
}
