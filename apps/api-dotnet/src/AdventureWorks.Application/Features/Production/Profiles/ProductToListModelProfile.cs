using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductToListModelProfile : Profile
{
    public ProductToListModelProfile()
    {
        CreateMap<Product, ProductListModel>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.ProductId))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.ProductSubcategory != null && s.ProductSubcategory.ProductCategory != null
                ? s.ProductSubcategory.ProductCategory.Name : null))
            .ForMember(d => d.SubcategoryName, o => o.MapFrom(s => s.ProductSubcategory != null ? s.ProductSubcategory.Name : null))
            .ForMember(d => d.ModelName, o => o.MapFrom(s => s.ProductModel != null ? s.ProductModel.Name : null))
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.DiscontinuedDate == null));
    }
}
