using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadShiftQuery : IRequest<ShiftModel>
{
    public required byte Id { get; set; } = byte.MinValue;
}
