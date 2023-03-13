using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles;

public sealed class BusinessEntityAddressEntityToModelProfile : Profile
{
    public BusinessEntityAddressEntityToModelProfile()
    {
        CreateMap<BusinessEntityAddressEntity, BusinessEntityAddressModel>()

            .ForPath(x => x.Address,
                o => o.MapFrom(y => y.Address))

            .ForPath(x => x.AddressType,
                o => o.MapFrom(y => y.AddressType));
    }
}
