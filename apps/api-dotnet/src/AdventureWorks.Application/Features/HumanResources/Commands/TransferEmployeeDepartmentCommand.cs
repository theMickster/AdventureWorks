using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>Command to transfer an employee to a different department and/or shift.</summary>
public sealed class TransferEmployeeDepartmentCommand : IRequest<Unit>
{
    /// <summary>The employee's business entity identifier. Set from route by controller.</summary>
    public required int EmployeeId { get; init; }

    /// <summary>The transfer request model.</summary>
    public required EmployeeTransferModel Model { get; init; }

    /// <summary>The audit timestamp for ModifiedDate fields.</summary>
    public required DateTime ModifiedDate { get; init; }

    /// <summary>The transfer date used as StartDate on the new history record. Should be date-only (no time).</summary>
    public required DateTime TransferDate { get; init; }
}
