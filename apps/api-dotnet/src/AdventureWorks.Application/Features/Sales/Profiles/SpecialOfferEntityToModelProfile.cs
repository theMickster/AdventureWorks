using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class SpecialOfferEntityToModelProfile : Profile
{
    public SpecialOfferEntityToModelProfile()
    {
        CreateMap<SpecialOffer, SpecialOfferModel>()
            .ForPath(a => a.SpecialOfferId,
                o => o.MapFrom(x => x.SpecialOfferId))

            .ForPath(a => a.Description,
                o => o.MapFrom(x => x.Description))

            .ForPath(a => a.DiscountPct,
                o => o.MapFrom(x => x.DiscountPct))

            .ForPath(a => a.Type,
                o => o.MapFrom(x => x.Type))

            .ForPath(a => a.Category,
                o => o.MapFrom(x => x.Category))

            .ForPath(a => a.StartDate,
                o => o.MapFrom(x => x.StartDate))

            .ForPath(a => a.EndDate,
                o => o.MapFrom(x => x.EndDate))

            .ForPath(a => a.MinQty,
                o => o.MapFrom(x => x.MinQty))

            .ForPath(a => a.MaxQty,
                o => o.MapFrom(x => x.MaxQty))

            .ForPath(a => a.IsActive,
                o => o.MapFrom(x => x.StartDate.Date <= DateTime.Now.Date && x.EndDate.Date >= DateTime.Now.Date))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
