using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class BusinessEntityAddressEntityToStoreAddressModelProfile : Profile
{
    public BusinessEntityAddressEntityToStoreAddressModelProfile()
    {
        CreateMap<BusinessEntityAddressEntity, StoreAddressModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AddressId))
            .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.BusinessEntityId))
            .ForMember(dest => dest.AddressTypeId, opt => opt.MapFrom(src => src.AddressTypeId))
            .ForMember(dest => dest.AddressTypeName, opt => opt.MapFrom(src => src.AddressType.Name))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.Address.AddressLine1))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.Address.AddressLine2))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.Address.PostalCode))
            .ForMember(dest => dest.StateProvinceCode, opt => opt.MapFrom(src => src.Address.StateProvince.StateProvinceCode))
            .ForMember(dest => dest.StateProvinceName, opt => opt.MapFrom(src => src.Address.StateProvince.Name))
            .ForMember(dest => dest.CountryRegionCode, opt => opt.MapFrom(src => src.Address.StateProvince.CountryRegion.CountryRegionCode))
            .ForMember(dest => dest.CountryRegionName, opt => opt.MapFrom(src => src.Address.StateProvince.CountryRegion.Name))
            .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate));
    }
}
