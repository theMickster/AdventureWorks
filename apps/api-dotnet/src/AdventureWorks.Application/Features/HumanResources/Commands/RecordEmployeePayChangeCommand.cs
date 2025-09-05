using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

public sealed class RecordEmployeePayChangeCommand : IRequest<Unit>
{
    public required int EmployeeId { get; init; }
    public required EmployeePayChangeCreateModel Model { get; init; }
    public required DateTime ModifiedDate { get; init; }
    public required DateTime RateChangeDate { get; init; }
}
