using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class LocationToModelProfile : Profile
{
    public LocationToModelProfile()
    {
        CreateMap<Location, LocationModel>()
            .ForPath(a => a.LocationId,
                o => o.MapFrom(x => x.LocationId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.CostRate,
                o => o.MapFrom(x => x.CostRate))

            .ForPath(a => a.Availability,
                o => o.MapFrom(x => x.Availability))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
