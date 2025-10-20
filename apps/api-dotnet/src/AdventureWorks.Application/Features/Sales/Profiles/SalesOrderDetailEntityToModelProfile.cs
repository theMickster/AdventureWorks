using AdventureWorks.Application.Features.Sales;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

/// <summary>
/// AutoMapper profile for mapping SalesOrderHeader to SalesOrderDetailModel and its child DTOs.
/// </summary>
public sealed class SalesOrderDetailEntityToModelProfile : Profile
{
    /// <summary>
    /// Registers AutoMapper maps for <see cref="SalesOrderHeader"/> → <see cref="SalesOrderDetailModel"/> and its child DTOs.
    /// </summary>
    public SalesOrderDetailEntityToModelProfile()
    {
        CreateMap<SalesOrderHeader, SalesOrderDetailModel>()
            .ForMember(dest => dest.StatusDescription, opt => opt.MapFrom<DetailStatusDescriptionResolver>())
            .ForMember(dest => dest.SalesPersonName, opt => opt.MapFrom<DetailSalesPersonNameResolver>())
            .ForMember(dest => dest.TerritoryName, opt => opt.MapFrom(src => src.TerritoryEntity != null ? src.TerritoryEntity.Name : null))
            .ForMember(dest => dest.BillToAddress, opt => opt.MapFrom(src => src.BillToAddressEntity))
            .ForMember(dest => dest.ShipToAddress, opt => opt.MapFrom(src => src.ShipToAddressEntity))
            .ForMember(dest => dest.LineItems, opt => opt.MapFrom(src => src.SalesOrderDetails));

        CreateMap<SalesOrderDetail, SalesOrderLineItemModel>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        CreateMap<AddressEntity, SalesOrderAddressModel>()
            .ForMember(dest => dest.StateProvince, opt => opt.MapFrom(src => src.StateProvince != null ? src.StateProvince.Name : string.Empty));
    }
}

/// <summary>
/// Resolves the human-readable description for a sales order status code.
/// </summary>
internal sealed class DetailStatusDescriptionResolver : IValueResolver<SalesOrderHeader, SalesOrderDetailModel, string>
{
    /// <summary>
    /// Resolves the status description for the destination model from the source entity's status byte.
    /// </summary>
    /// <param name="src">the source sales order header entity</param>
    /// <param name="dest">the destination detail model being constructed</param>
    /// <param name="destMember">the current destination member value</param>
    /// <param name="context">the current AutoMapper resolution context</param>
    /// <returns>A human-readable order status string</returns>
    public string Resolve(SalesOrderHeader src, SalesOrderDetailModel dest, string destMember, ResolutionContext context) =>
        SalesOrderResolverHelpers.GetStatusDescription(src.Status);
}

/// <summary>
/// Resolves the sales person's full name from the sales order entity.
/// </summary>
internal sealed class DetailSalesPersonNameResolver : IValueResolver<SalesOrderHeader, SalesOrderDetailModel, string?>
{
    /// <summary>
    /// Resolves the sales person's full name from the source entity graph, returning null when unassigned.
    /// </summary>
    /// <param name="src">the source sales order header entity</param>
    /// <param name="dest">the destination detail model being constructed</param>
    /// <param name="destMember">the current destination member value</param>
    /// <param name="context">the current AutoMapper resolution context</param>
    /// <returns>A "FirstName LastName" string, or null if the order has no assigned sales person</returns>
    public string? Resolve(SalesOrderHeader src, SalesOrderDetailModel dest, string? destMember, ResolutionContext context) =>
        SalesOrderResolverHelpers.GetSalesPersonName(src.SalesPerson);
}
