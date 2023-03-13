using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Models.Sales;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles.Sales;

public sealed class StoreEntityToModelProfile : Profile
{
    public StoreEntityToModelProfile()
    {
        CreateMap<StoreEntity, StoreModel>()
            .ForPath(x => x.Id, o => o.MapFrom(y => y.BusinessEntityId))

            .ForPath(x => x.Name, o => o.MapFrom(y => y.Name))

            .ForPath(x => x.ModifiedDate, o => o.MapFrom(y => y.ModifiedDate))
            
            .ForPath( x => x.StoreAddresses, o => o.MapFrom(y => y.StoreBusinessEntity.BusinessEntityAddresses));
    }
}
