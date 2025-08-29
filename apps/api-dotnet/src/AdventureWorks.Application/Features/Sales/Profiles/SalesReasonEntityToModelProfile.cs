using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class SalesReasonEntityToModelProfile : Profile
{
    public SalesReasonEntityToModelProfile()
    {
        CreateMap<SalesReason, SalesReasonModel>()
            .ForPath(a => a.SalesReasonId,
                o => o.MapFrom(x => x.SalesReasonId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.ReasonType,
                o => o.MapFrom(x => x.ReasonType))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
