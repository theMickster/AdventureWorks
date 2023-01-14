using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles;

public sealed class AddressCreateModelToAddressEntity : Profile
{
    public AddressCreateModelToAddressEntity()
    {

        CreateMap<AddressCreateModel, AddressEntity>()

            .ForPath(m => m.StateProvinceId,
                options
                    => options.MapFrom(e => e.StateProvince.Id))
        
            .ForMember(x => x.ModifiedDate, 
                o => o.Ignore())

            .ForMember(x => x.AddressId,
                o => o.Ignore())

            .ForMember(x => x.Rowguid,
                o => o.Ignore())

            .ForMember(x => x.SalesOrderHeaderBillToAddresses,
                o => o.Ignore())

            .ForMember(x => x.SalesOrderHeaderShipToAddress,
                o => o.Ignore())

            .ForMember(x => x.StateProvince,
                o => o.Ignore());
    }
}