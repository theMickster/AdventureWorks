using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;

namespace AdventureWorks.Application.Features.AddressManagement.Profiles;

public sealed class AddressUpdateModelToAddressEntityProfile : Profile
{
    public AddressUpdateModelToAddressEntityProfile()
    {
        CreateMap<AddressUpdateModel, AddressEntity>()

            .ForPath(m => m.StateProvinceId,
                options
                    => options.MapFrom(e => e.AddressStateProvince.Id))

            .ForPath(x => x.AddressId,
                options => options.MapFrom(e => e.Id))

            .ForMember(x => x.ModifiedDate,
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