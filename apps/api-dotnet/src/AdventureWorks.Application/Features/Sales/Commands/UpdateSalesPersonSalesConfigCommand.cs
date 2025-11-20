using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// MediatR command that patches the sales configuration fields of an existing sales person.
/// </summary>
public sealed class UpdateSalesPersonSalesConfigCommand : IRequest
{
    /// <summary>
    /// The validated patch payload containing the sales person ID and updated fields.
    /// </summary>
    public required SalesPersonSalesConfigUpdateModel Model { get; set; }

    /// <summary>
    /// The UTC timestamp applied to the entity's ModifiedDate audit field.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// The identity of the user who initiated the update, sourced from the JWT claim.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}
