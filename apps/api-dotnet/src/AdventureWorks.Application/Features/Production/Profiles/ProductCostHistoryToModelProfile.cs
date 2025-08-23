using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductCostHistoryToModelProfile : Profile
{
    public ProductCostHistoryToModelProfile()
    {
        CreateMap<ProductCostHistory, ProductPriceHistoryModel>()
            .ForMember(d => d.Price, o => o.MapFrom(s => s.StandardCost))
            .ForMember(d => d.Type, o => o.MapFrom(_ => "cost"));
    }
}
