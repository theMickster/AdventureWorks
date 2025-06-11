namespace AdventureWorks.Models.Features.Sales;

public sealed class StoreUpdateModel : StoreBaseModel
{
    public int Id { get; set; }

    public int? SalesPersonId { get; set; }
}
