using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadEmployeeOrgTreeQuery : IRequest<IReadOnlyList<EmployeeOrgTreeItemModel>>
{
}
