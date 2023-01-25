using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles;

public sealed class AddressEntityToAddressModelProfile : Profile
{
    public AddressEntityToAddressModelProfile()
    {
        CreateMap<AddressEntity, AddressModel>()
            .ForPath(m => m.Id,
                options
                    => options.MapFrom(e => e.AddressId))

            .ForPath(m => m.StateProvince.Id,
                options
                    => options.MapFrom(e => e.StateProvince.StateProvinceId))

            .ForPath(m => m.StateProvince.StateProvinceCode,
                options
                    => options.MapFrom(e => e.StateProvince.StateProvinceCode))

            .ForPath(m => m.StateProvince.CountryRegionCode,
                options
                    => options.MapFrom(e => e.StateProvince.CountryRegionCode))

            .ForPath(m => m.CountryRegion.Code,
                options
                    => options.MapFrom(e => e.StateProvince.CountryRegionCode))

            .ForPath(m => m.CountryRegion.Name,
                options
                    => options.MapFrom(e => e.StateProvince.CountryRegion.Name));


    }

}