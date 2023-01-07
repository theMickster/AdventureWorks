﻿using AdventureWorks.Domain.Entities;
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

            .ForPath(m => m.StateProvinceCode,
                options
                    => options.MapFrom(e => e.StateProvince.StateProvinceCode))

            .ForPath(m => m.CountryRegionCode,
                options
                    => options.MapFrom(e => e.StateProvince.CountryRegionCode))

            .ForPath(m => m.CountryRegionName,
                options
                    => options.MapFrom(e => e.StateProvince.CountryRegionCodeNavigation.Name));
    }
}