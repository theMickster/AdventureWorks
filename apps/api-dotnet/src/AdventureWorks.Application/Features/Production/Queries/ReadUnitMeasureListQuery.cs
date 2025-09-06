using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadUnitMeasureListQuery : IRequest<List<UnitMeasureModel>>
{
}
