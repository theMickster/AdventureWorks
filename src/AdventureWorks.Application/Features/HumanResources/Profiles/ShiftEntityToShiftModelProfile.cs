using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

public sealed class ShiftEntityToShiftModelProfile : Profile
{
    public ShiftEntityToShiftModelProfile()
    {
        CreateMap<ShiftEntity, ShiftModel>()
            .ForPath(a => a.Id,
                o => o.MapFrom(x => x.ShiftId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.StartTime,
                o => o.MapFrom(x => x.StartTime))

            .ForPath(a => a.EndTime,
                o => o.MapFrom(x => x.EndTime))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
