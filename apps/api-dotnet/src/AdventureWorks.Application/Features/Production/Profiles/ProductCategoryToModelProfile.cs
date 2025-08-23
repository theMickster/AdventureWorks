using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductCategoryToModelProfile : Profile
{
    public ProductCategoryToModelProfile()
    {
        CreateMap<ProductCategory, ProductCategoryModel>()
            .ForMember(d => d.CategoryId, o => o.MapFrom(s => s.ProductCategoryId));
    }
}
