using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductUpdateModelToProductProfile : Profile
{
    public ProductUpdateModelToProductProfile()
    {
        CreateMap<ProductUpdateModel, Product>()
            .ForMember(x => x.ProductId, o => o.MapFrom(s => s.Id))
            .ForMember(x => x.Rowguid, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.BillOfMaterialsComponents, o => o.Ignore())
            .ForMember(x => x.BillOfMaterialsProductAssemblies, o => o.Ignore())
            .ForMember(x => x.ProductCostHistory, o => o.Ignore())
            .ForMember(x => x.ProductInventory, o => o.Ignore())
            .ForMember(x => x.ProductListPriceHistory, o => o.Ignore())
            .ForMember(x => x.ProductProductPhotos, o => o.Ignore())
            .ForMember(x => x.ProductReviews, o => o.Ignore())
            .ForMember(x => x.ProductVendors, o => o.Ignore())
            .ForMember(x => x.PurchaseOrderDetails, o => o.Ignore())
            .ForMember(x => x.ShoppingCartItems, o => o.Ignore())
            .ForMember(x => x.SpecialOfferProducts, o => o.Ignore())
            .ForMember(x => x.TransactionHistory, o => o.Ignore())
            .ForMember(x => x.WorkOrderRoutings, o => o.Ignore())
            .ForMember(x => x.WorkOrders, o => o.Ignore())
            .ForMember(x => x.ProductModel, o => o.Ignore())
            .ForMember(x => x.ProductSubcategory, o => o.Ignore())
            .ForMember(x => x.SizeUnitMeasureCodeNavigation, o => o.Ignore())
            .ForMember(x => x.WeightUnitMeasureCodeNavigation, o => o.Ignore());
    }
}
