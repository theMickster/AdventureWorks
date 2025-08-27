using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StoreAddressCreateModelToBusinessEntityAddressEntityProfile : Profile
{
    public StoreAddressCreateModelToBusinessEntityAddressEntityProfile()
    {
        CreateMap<StoreAddressCreateModel, BusinessEntityAddressEntity>()
            .ForMember(x => x.AddressId, o => o.MapFrom(y => y.AddressId))
            .ForMember(x => x.AddressTypeId, o => o.MapFrom(y => y.AddressTypeId))
            .ForMember(x => x.BusinessEntityId, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.Rowguid, o => o.Ignore())
            .ForMember(x => x.BusinessEntity, o => o.Ignore())
            .ForMember(x => x.Address, o => o.Ignore())
            .ForMember(x => x.AddressType, o => o.Ignore());
    }
}
