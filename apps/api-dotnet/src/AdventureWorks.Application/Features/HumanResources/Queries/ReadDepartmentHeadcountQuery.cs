using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadDepartmentHeadcountQuery : IRequest<DepartmentHeadcountModel>
{
    public required short DepartmentId { get; init; }
}
