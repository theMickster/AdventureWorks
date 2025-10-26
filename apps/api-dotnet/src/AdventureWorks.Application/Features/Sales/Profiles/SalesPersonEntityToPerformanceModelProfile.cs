using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class SalesPersonEntityToPerformanceModelProfile : Profile
{
    public SalesPersonEntityToPerformanceModelProfile()
    {
        CreateMap<SalesPersonEntity, SalesPersonPerformanceModel>()
            .ForMember(dest => dest.OrderCount, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRevenue, opt => opt.Ignore())
            .ForMember(dest => dest.QuotaHistory, opt => opt.MapFrom(src => src.SalesPersonQuotaHistory))
            .ForMember(dest => dest.TerritoryHistory, opt => opt.MapFrom(src => src.SalesTerritoryHistory));

        CreateMap<SalesPersonQuotaHistoryEntity, SalesPersonQuotaHistoryModel>();

        CreateMap<SalesTerritoryHistory, SalesPersonTerritoryHistoryModel>()
            .ForMember(dest => dest.TerritoryName, opt => opt.MapFrom(src => src.TerritoryEntity != null ? src.TerritoryEntity.Name : string.Empty));
    }
}
