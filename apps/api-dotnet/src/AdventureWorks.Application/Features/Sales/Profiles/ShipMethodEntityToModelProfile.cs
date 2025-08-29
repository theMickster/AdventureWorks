using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class ShipMethodEntityToModelProfile : Profile
{
    public ShipMethodEntityToModelProfile()
    {
        CreateMap<ShipMethod, ShipMethodModel>()
            .ForPath(a => a.ShipMethodId,
                o => o.MapFrom(x => x.ShipMethodId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.ShipBase,
                o => o.MapFrom(x => x.ShipBase))

            .ForPath(a => a.ShipRate,
                o => o.MapFrom(x => x.ShipRate))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
