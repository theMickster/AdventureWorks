namespace AdventureWorks.Domain.Entities;

public sealed class SalesTaxRateEntity : BaseEntity
{
    public int SalesTaxRateId { get; set; }
    
    public int StateProvinceId { get; set; }
    
    public byte TaxType { get; set; }
    
    public decimal TaxRate { get; set; }
    
    public string Name { get; set; }
    
    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }

    public StateProvinceEntity StateProvince { get; set; }
}