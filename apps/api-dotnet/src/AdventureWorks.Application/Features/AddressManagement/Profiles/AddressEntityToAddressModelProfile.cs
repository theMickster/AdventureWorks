using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.Application.Features.AddressManagement.Profiles;

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

            .ForPath(m => m.StateProvince.Code,
                options
                    => options.MapFrom(e => e.StateProvince.StateProvinceCode.Trim()))

            .ForPath(m => m.StateProvince.Name,
             options => options.MapFrom(e => e.StateProvince.Name.Trim()))

            .ForPath(m => m.CountryRegion.Code,
                options
                    => options.MapFrom(e => e.StateProvince.CountryRegionCode))

            .ForPath(m => m.CountryRegion.Name,
                options
                    => options.MapFrom(e => e.StateProvince.CountryRegion.Name));


    }

}