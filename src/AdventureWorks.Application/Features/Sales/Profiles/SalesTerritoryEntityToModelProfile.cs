using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class SalesTerritoryEntityToModelProfile : Profile
{
    public SalesTerritoryEntityToModelProfile()
    {
        CreateMap<SalesTerritoryEntity, SalesTerritoryModel>()
            .ForPath(a => a.Id,
                o => o.MapFrom(x => x.TerritoryId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.Group,
                o => o.MapFrom(x => x.Group))

            .ForPath(a => a.SalesYtd,
                o => o.MapFrom(x => x.SalesYtd))

            .ForPath(a => a.SalesLastYear,
                o => o.MapFrom(x => x.SalesLastYear))

            .ForPath(a => a.CostYtd,
                o => o.MapFrom(x => x.CostYtd))

            .ForPath(a => a.CostLastYear,
                o => o.MapFrom(x => x.CostLastYear))

            .ForPath(a => a.CountryRegion.Name,
                o => o.MapFrom(x => x.CountryRegion.Name))

            .ForPath(a => a.CountryRegion.Code,
                o => o.MapFrom(x => x.CountryRegion.CountryRegionCode));
    }
}
