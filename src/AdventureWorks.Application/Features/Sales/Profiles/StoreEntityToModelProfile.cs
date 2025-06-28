using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StoreEntityToModelProfile : Profile
{
    public StoreEntityToModelProfile()
    {
        CreateMap<StoreEntity, StoreModel>()
            .ForMember(x => x.Id, o => o.MapFrom(y => y.BusinessEntityId))

            .ForMember(x => x.Name, o => o.MapFrom(y => y.Name))

            .ForMember(x => x.ModifiedDate, o => o.MapFrom(y => y.ModifiedDate))

            .ForMember(x => x.StoreAddresses, o => o.MapFrom(y => y.StoreBusinessEntity.BusinessEntityAddresses))

            .ForMember(x => x.StoreContacts, o => o.Ignore())

            .ForMember(x => x.SalesPerson, o => o.MapFrom(y => y.PrimarySalesPerson));
    }
}
