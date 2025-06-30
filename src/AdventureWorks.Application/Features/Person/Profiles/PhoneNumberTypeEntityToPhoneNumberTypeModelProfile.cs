using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

public sealed class PhoneNumberTypeEntityToPhoneNumberTypeModelProfile : Profile
{
    public PhoneNumberTypeEntityToPhoneNumberTypeModelProfile()
    {
        CreateMap<PhoneNumberTypeEntity, PhoneNumberTypeModel>()
            .ForPath(a => a.Id,
                o => o.MapFrom(x => x.PhoneNumberTypeId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
