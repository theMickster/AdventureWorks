using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.Application.Features.Sales;

internal static class SalesOrderResolverHelpers
{
    /// <summary>
    /// Maps the raw status byte to its human-readable description.
    /// </summary>
    /// <param name="status">the order status code (1–6)</param>
    /// <returns>A descriptive string such as "Shipped" or "Unknown" for unrecognised codes</returns>
    internal static string GetStatusDescription(byte status) =>
        status switch
        {
            1 => "In process",
            2 => "Approved",
            3 => "Backordered",
            4 => "Rejected",
            5 => "Shipped",
            6 => "Cancelled",
            _ => "Unknown"
        };

    /// <summary>
    /// Builds the full name for a sales person by traversing the entity graph to the linked <c>PersonBusinessEntity</c>.
    /// </summary>
    /// <param name="salesPerson">the sales person entity, or null if the order has no assigned sales person</param>
    /// <returns>A "FirstName LastName" string, or null when the sales person or underlying person data is absent</returns>
    internal static string? GetSalesPersonName(SalesPersonEntity? salesPerson)
    {
        if (salesPerson?.Employee?.PersonBusinessEntity is null)
        {
            return null;
        }

        var person = salesPerson.Employee.PersonBusinessEntity;
        return $"{person.FirstName} {person.LastName}".Trim();
    }
}
