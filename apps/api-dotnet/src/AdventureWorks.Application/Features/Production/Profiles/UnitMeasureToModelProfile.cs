using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class UnitMeasureToModelProfile : Profile
{
    public UnitMeasureToModelProfile()
    {
        CreateMap<UnitMeasure, UnitMeasureModel>()
            .ForPath(a => a.UnitMeasureCode,
                o => o.MapFrom(x => x.UnitMeasureCode))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
