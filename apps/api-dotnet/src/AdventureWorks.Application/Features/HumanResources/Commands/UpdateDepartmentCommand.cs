using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

public sealed class UpdateDepartmentCommand : IRequest<Unit>
{
    public required DepartmentUpdateModel Model { get; set; }
    public DateTime ModifiedDate { get; set; }
}
