using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadDepartmentQuery : IRequest<DepartmentModel>
{
    public required short Id { get; set; } = short.MinValue;
}
