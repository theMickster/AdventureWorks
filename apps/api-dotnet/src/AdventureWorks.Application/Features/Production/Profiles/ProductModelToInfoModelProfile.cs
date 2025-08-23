using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductModelToInfoModelProfile : Profile
{
    public ProductModelToInfoModelProfile()
    {
        CreateMap<Domain.Entities.Production.ProductModel, ProductModelInfoModel>()
            .ForMember(d => d.ModelId, o => o.MapFrom(s => s.ProductModelId));
    }
}
