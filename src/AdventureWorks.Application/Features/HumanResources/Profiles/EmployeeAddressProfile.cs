using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Models.Slim;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

/// <summary>
/// AutoMapper profile for mapping between BusinessEntityAddressEntity and EmployeeAddressModel.
/// </summary>
public sealed class EmployeeAddressProfile : Profile
{
    public EmployeeAddressProfile()
    {
        // Map BusinessEntityAddressEntity → EmployeeAddressModel
        CreateMap<BusinessEntityAddressEntity, EmployeeAddressModel>()
            .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => src.Address.AddressId))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.Address.AddressLine1))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.Address.AddressLine2))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.Address.PostalCode))
            .ForMember(dest => dest.StateProvince, opt => opt.MapFrom(src => new GenericSlimModel
            {
                Id = src.Address.StateProvince.StateProvinceId,
                Name = src.Address.StateProvince.Name,
                Code = src.Address.StateProvince.StateProvinceCode
            }))
            .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => new GenericSlimModel
            {
                Id = src.AddressType.AddressTypeId,
                Name = src.AddressType.Name,
                Code = string.Empty
            }))
            .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.Address.ModifiedDate));

        // Map EmployeeAddressUpdateModel → AddressEntity (for updates)
        CreateMap<EmployeeAddressUpdateModel, AddressEntity>()
            .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => src.AddressId))
            .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.AddressLine1))
            .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.AddressLine2))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.StateProvinceId, opt => opt.MapFrom(src => src.StateProvinceId))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
            .ForMember(dest => dest.Rowguid, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.SalesOrderHeaderBillToAddresses, opt => opt.Ignore())
            .ForMember(dest => dest.SalesOrderHeaderShipToAddress, opt => opt.Ignore())
            .ForMember(dest => dest.StateProvince, opt => opt.Ignore());
    }
}
