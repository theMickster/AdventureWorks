namespace AdventureWorks.Domain.Models.Sales;

public sealed class StoreContactModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public string Suffix { get; set; }

    public int ContactTypeId { get; set; }

    public string ContactTypeName { get; set; }
}
