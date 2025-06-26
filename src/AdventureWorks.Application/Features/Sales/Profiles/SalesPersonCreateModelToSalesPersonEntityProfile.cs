using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class SalesPersonCreateModelToSalesPersonEntityProfile : Profile
{
    public SalesPersonCreateModelToSalesPersonEntityProfile()
    {
        CreateMap<SalesPersonCreateModel, SalesPersonEntity>()
            .ForMember(x => x.BusinessEntityId, options => options.MapFrom(y => y.BusinessEntityId))
            .ForMember(x => x.TerritoryId, options => options.MapFrom(y => y.TerritoryId))
            .ForMember(x => x.SalesQuota, options => options.MapFrom(y => y.SalesQuota))
            .ForMember(x => x.Bonus, options => options.MapFrom(y => y.Bonus))
            .ForMember(x => x.CommissionPct, options => options.MapFrom(y => y.CommissionPct))

            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.Rowguid, o => o.Ignore())
            .ForMember(x => x.SalesYtd, o => o.Ignore())
            .ForMember(x => x.SalesLastYear, o => o.Ignore())
            .ForMember(x => x.SalesOrderHeaders, o => o.Ignore())
            .ForMember(x => x.SalesPersonQuotaHistory, o => o.Ignore())
            .ForMember(x => x.SalesTerritoryHistory, o => o.Ignore())
            .ForMember(x => x.Employee, o => o.Ignore())
            .ForMember(x => x.SalesTerritory, o => o.Ignore())
            .ReverseMap();
    }
}
