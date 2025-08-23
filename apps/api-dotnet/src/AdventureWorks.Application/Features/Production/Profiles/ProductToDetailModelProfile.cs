using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductToDetailModelProfile : Profile
{
    public ProductToDetailModelProfile()
    {
        CreateMap<Product, ProductDetailModel>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.ProductId))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.ProductSubcategory != null ? s.ProductSubcategory.ProductCategory : null))
            .ForMember(d => d.Subcategory, o => o.MapFrom(s => s.ProductSubcategory))
            .ForMember(d => d.Model, o => o.MapFrom(s => s.ProductModel))
            .ForMember(d => d.Photos, o => o.MapFrom(s => s.ProductProductPhotos))
            .ForMember(d => d.TotalInventoryQuantity, o => o.MapFrom(s => s.ProductInventory != null ? s.ProductInventory.Sum(i => (int)i.Quantity) : 0));
    }
}
