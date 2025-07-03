using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Command to create a new employee with person data and optional contact information.
/// Returns the BusinessEntityId of the created employee.
/// </summary>
public sealed class CreateEmployeeCommand : IRequest<int>
{
    /// <summary>
    /// Employee creation data including person information, employee details,
    /// and optional phone, email, and address.
    /// </summary>
    public required EmployeeCreateModel Model { get; set; }

    /// <summary>
    /// System-generated modification timestamp.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// System-generated unique row identifier.
    /// </summary>
    public Guid RowGuid { get; set; }
}
