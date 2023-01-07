using System;
using System.Collections.Generic;

namespace AdventureWorks.Domain.Entities;

public class StateProvince : BaseEntity
{

    public int StateProvinceId { get; set; }
    public string StateProvinceCode { get; set; }
    public string CountryRegionCode { get; set; }
    public bool IsOnlyStateProvinceFlag { get; set; }
    public string Name { get; set; }
    public int TerritoryId { get; set; }
    public Guid Rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<AddressEntity> Address { get; set; }
    public virtual ICollection<SalesTaxRate> SalesTaxRate { get; set; }
    public virtual CountryRegion CountryRegionCodeNavigation { get; set; }
    public virtual SalesTerritory Territory { get; set; }
}