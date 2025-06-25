using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StoreCreateModelToStoreEntityProfile :Profile
{
    public StoreCreateModelToStoreEntityProfile()
    {
        CreateMap<StoreCreateModel, StoreEntity>()
            .ForMember(x => x.Name, options => options.MapFrom(y => y.Name))
            .ForMember(x => x.SalesPersonId, options => options.MapFrom(y => y.SalesPersonId))

            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.BusinessEntityId, o => o.Ignore())
            .ForMember(x => x.Rowguid, o => o.Ignore())
            .ForMember(x => x.Customers, o => o.Ignore())
            .ForMember(x => x.Demographics, o => o.Ignore())
            .ForMember(x => x.PrimarySalesPerson, o => o.Ignore())
            .ForMember(x => x.StoreBusinessEntity, o => o.Ignore())
            .ReverseMap();
    }
}
