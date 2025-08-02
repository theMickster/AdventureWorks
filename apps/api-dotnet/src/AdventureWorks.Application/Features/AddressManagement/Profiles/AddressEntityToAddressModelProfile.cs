using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Slim;
using AutoMapper;

namespace AdventureWorks.Application.Features.AddressManagement.Profiles;

public sealed class AddressEntityToAddressModelProfile : Profile
{
    public AddressEntityToAddressModelProfile()
    {
        CreateMap<AddressEntity, AddressModel>()
            .ForPath(m => m.Id,
                options
                    => options.MapFrom(e => e.AddressId))

            .ForMember(m => m.StateProvince,
                options
                    => options.MapFrom(e => new GenericSlimModel
                    {
                        Id = e.StateProvince!.StateProvinceId,
                        Code = (e.StateProvince.StateProvinceCode ?? string.Empty).Trim(),
                        Name = (e.StateProvince.Name ?? string.Empty).Trim()
                    }))

            .ForMember(m => m.CountryRegion,
                options
                    => options.MapFrom(e => new CountryRegionModel
                    {
                        Code = e.StateProvince!.CountryRegionCode ?? string.Empty,
                        Name = e.StateProvince.CountryRegion!.Name ?? string.Empty
                    }));


    }

}