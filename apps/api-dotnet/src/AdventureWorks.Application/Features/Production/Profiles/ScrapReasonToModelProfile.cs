using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ScrapReasonToModelProfile : Profile
{
    public ScrapReasonToModelProfile()
    {
        CreateMap<ScrapReason, ScrapReasonModel>()
            .ForPath(a => a.ScrapReasonId,
                o => o.MapFrom(x => x.ScrapReasonId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
