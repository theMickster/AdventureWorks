using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles;

public sealed class AddressTypeEntityToModelProfile : Profile
{
    public AddressTypeEntityToModelProfile()
    {
        CreateMap<AddressTypeEntity, AddressTypeModel>()
            .ForPath(a => a.Id,
                o => o.MapFrom(x => x.AddressTypeId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name));
    }
}
