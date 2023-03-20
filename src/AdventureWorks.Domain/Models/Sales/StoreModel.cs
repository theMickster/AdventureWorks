namespace AdventureWorks.Domain.Models.Sales;

public sealed class StoreModel : StoreBaseModel
{
    public int Id { get; set; }

    public DateTime ModifiedDate { get; set; }

    public List<BusinessEntityAddressModel> StoreAddresses { get; set; }

    public List<StoreContactModel> StoreContacts { get; set; }
}
