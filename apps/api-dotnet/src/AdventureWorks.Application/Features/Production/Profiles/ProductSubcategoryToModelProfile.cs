using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductSubcategoryToModelProfile : Profile
{
    public ProductSubcategoryToModelProfile()
    {
        CreateMap<ProductSubcategory, ProductSubcategoryModel>()
            .ForMember(d => d.SubcategoryId, o => o.MapFrom(s => s.ProductSubcategoryId))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.ProductCategory != null ? s.ProductCategory.Name : null));
    }
}
