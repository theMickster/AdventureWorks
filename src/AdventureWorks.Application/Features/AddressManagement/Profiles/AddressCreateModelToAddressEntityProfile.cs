using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.Application.Features.AddressManagement.Profiles;

public sealed class AddressCreateModelToAddressEntityProfile : Profile
{
    public AddressCreateModelToAddressEntityProfile()
    {

        CreateMap<AddressCreateModel, AddressEntity>()

            .ForPath(m => m.StateProvinceId,
                options
                    => options.MapFrom(e => e.AddressStateProvince.Id))
        
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