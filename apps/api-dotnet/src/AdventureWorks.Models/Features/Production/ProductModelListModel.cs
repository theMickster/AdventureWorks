namespace AdventureWorks.Models.Features.Production;

public sealed class ProductModelListModel
{
    public int ProductModelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime ModifiedDate { get; set; }
}
