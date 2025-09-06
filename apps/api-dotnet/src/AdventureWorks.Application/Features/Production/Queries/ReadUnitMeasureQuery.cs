using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadUnitMeasureQuery : IRequest<UnitMeasureModel?>
{
    public required string Code { get; set; } = string.Empty;
}
