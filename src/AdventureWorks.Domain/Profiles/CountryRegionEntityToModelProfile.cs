using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles;

public sealed class CountryRegionEntityToModelProfile : Profile
{
    public CountryRegionEntityToModelProfile()
    {
        CreateMap<CountryRegionEntity, CountryRegionModel>()
            .ForPath(c => c.Code,
                opt => opt.MapFrom(x => x.CountryRegionCode))

            .ForPath(c => c.Name,
                opt => opt.MapFrom(x => x.Name));
    }
}