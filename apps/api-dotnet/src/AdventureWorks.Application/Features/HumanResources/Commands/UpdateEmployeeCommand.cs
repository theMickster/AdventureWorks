using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to update an existing employee's personal information and marital status.
/// Updates both PersonEntity and EmployeeEntity tables.
/// Does not update immutable fields (NationalIdNumber, LoginId, BirthDate, HireDate).
/// </summary>
public sealed class UpdateEmployeeCommand : IRequest<Unit>
{
    /// <summary>
    /// Employee update data including person information and marital status.
    /// </summary>
    public required EmployeeUpdateModel Model { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
