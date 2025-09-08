using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

public sealed class CreateDepartmentCommand : IRequest<short>
{
    public required DepartmentCreateModel Model { get; set; }
    public DateTime ModifiedDate { get; set; }
}
