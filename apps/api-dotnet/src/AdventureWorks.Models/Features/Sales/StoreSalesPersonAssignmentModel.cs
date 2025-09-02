namespace AdventureWorks.Models.Features.Sales;

public sealed class StoreSalesPersonAssignmentModel
{
    public int SalesPersonId { get; set; }

    public string SalesPersonName { get; set; } = string.Empty;

    public string Territory { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
