namespace AdventureWorks.Models.Features.Sales;

public sealed class StoreContactModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Suffix { get; set; } = string.Empty;

    public int ContactTypeId { get; set; }

    public string ContactTypeName { get; set; } = string.Empty;

    public int StoreId { get; set; }
}
