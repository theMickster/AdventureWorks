using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.Application.Features.AddressManagement.Profiles;

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
