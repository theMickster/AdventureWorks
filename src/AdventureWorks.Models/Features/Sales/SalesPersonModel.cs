namespace AdventureWorks.Models.Features.Sales;

public sealed class SalesPersonModel : SalesPersonBaseModel
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public required string FirstName { get; set; }

    public string? MiddleName { get; set; }

    public required string LastName { get; set; }

    public string? Suffix { get; set; }

    public required string JobTitle { get; set; }

    public string? EmailAddress { get; set; }

    public DateTime ModifiedDate { get; set; }

}
