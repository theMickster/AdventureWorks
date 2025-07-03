using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

/// <summary>
/// AutoMapper profile for updating SalesPerson entities.
/// Maps ONLY SalesPerson-specific fields (TerritoryId, SalesQuota, Bonus, CommissionPct).
/// Person and Employee fields are updated manually in the command handler for explicit control.
/// </summary>
public sealed class SalesPersonUpdateModelToSalesPersonEntityProfile : Profile
{
    public SalesPersonUpdateModelToSalesPersonEntityProfile()
    {
        CreateMap<SalesPersonUpdateModel, SalesPersonEntity>()
            // Map SalesPerson-specific fields only
            .ForMember(x => x.TerritoryId, options => options.MapFrom(y => y.TerritoryId))
            .ForMember(x => x.SalesQuota, options => options.MapFrom(y => y.SalesQuota))
            .ForMember(x => x.Bonus, options => options.MapFrom(y => y.Bonus))
            .ForMember(x => x.CommissionPct, options => options.MapFrom(y => y.CommissionPct))

            // Ignore immutable and system-managed fields
            .ForMember(x => x.BusinessEntityId, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.Rowguid, o => o.Ignore())
            .ForMember(x => x.SalesYtd, o => o.Ignore())
            .ForMember(x => x.SalesLastYear, o => o.Ignore())

            // Ignore navigation properties and collections
            .ForMember(x => x.SalesOrderHeaders, o => o.Ignore())
            .ForMember(x => x.SalesPersonQuotaHistory, o => o.Ignore())
            .ForMember(x => x.SalesTerritoryHistory, o => o.Ignore())
            .ForMember(x => x.Employee, o => o.Ignore()) // Person/Employee updated manually in handler
            .ForMember(x => x.SalesTerritory, o => o.Ignore())
            .ReverseMap();
    }
}
