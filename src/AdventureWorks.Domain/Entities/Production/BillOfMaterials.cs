namespace AdventureWorks.Domain.Entities.Production;

public class BillOfMaterials : BaseEntity
{
    public int BillOfMaterialsId { get; set; }
    public int? ProductAssemblyId { get; set; }
    public int ComponentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string UnitMeasureCode { get; set; }
    public short Bomlevel { get; set; }
    public decimal PerAssemblyQty { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual Product Component { get; set; }
    public virtual Product ProductAssembly { get; set; }
    public virtual UnitMeasure UnitMeasureCodeNavigation { get; set; }
}