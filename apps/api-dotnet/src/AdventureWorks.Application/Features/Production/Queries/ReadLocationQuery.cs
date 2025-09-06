using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadLocationQuery : IRequest<LocationModel?>
{
    public required int Id { get; set; } = int.MinValue;
}
