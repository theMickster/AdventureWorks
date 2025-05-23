﻿using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.Application.Features.AddressManagement.Profiles;

public sealed class StateProvinceEntityToModelProfile : Profile
{
    public StateProvinceEntityToModelProfile()
    {
        CreateMap<StateProvinceEntity, StateProvinceModel>()
            .ForPath(c => c.Id,
                opt => opt.MapFrom(x => x.StateProvinceId))

            .ForPath(c => c.Code,
                opt => opt.MapFrom(x => x.StateProvinceCode))

            .ForPath( c => c.Name,
                opt => opt.MapFrom(x => x.Name))

            .ForPath(c => c.IsStateProvinceCodeUnavailable,
                opt => opt.MapFrom(x => x.IsOnlyStateProvinceFlag))
            
            .ForPath(c => c.CountryRegion.Code, 
                opt => opt.MapFrom(x => x.CountryRegion.CountryRegionCode))

            .ForPath(c => c.CountryRegion.Name,
                opt => opt.MapFrom(x => x.CountryRegion.Name))

            .ForPath(c => c.Territory.Id,
                opt => opt.MapFrom(x => x.SalesTerritory.TerritoryId))

            .ForPath(c => c.Territory.Name,
                opt => opt.MapFrom(x => x.SalesTerritory.Name))

            ;


    }
}
