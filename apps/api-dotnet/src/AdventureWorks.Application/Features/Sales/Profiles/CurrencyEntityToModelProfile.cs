using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class CurrencyEntityToModelProfile : Profile
{
    public CurrencyEntityToModelProfile()
    {
        CreateMap<Currency, CurrencyModel>()
            .ForPath(a => a.CurrencyCode,
                o => o.MapFrom(x => x.CurrencyCode))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
