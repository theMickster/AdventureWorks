using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductListPriceHistoryToModelProfile : Profile
{
    public ProductListPriceHistoryToModelProfile()
    {
        CreateMap<ProductListPriceHistory, ProductPriceHistoryModel>()
            .ForMember(d => d.Price, o => o.MapFrom(s => s.ListPrice))
            .ForMember(d => d.Type, o => o.MapFrom(_ => "list"));
    }
}
