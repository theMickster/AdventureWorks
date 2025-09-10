using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

public sealed class PersonPhoneEntityToPersonPhoneModelProfile : Profile
{
    public PersonPhoneEntityToPersonPhoneModelProfile()
    {
        CreateMap<PersonPhone, PersonPhoneModel>()
            .ForMember(x => x.PhoneNumber, o => o.MapFrom(y => y.PhoneNumber))
            .ForMember(x => x.PhoneNumberTypeId, o => o.MapFrom(y => y.PhoneNumberTypeId))
            .ForMember(x => x.PhoneNumberTypeName, o => o.MapFrom(y => y.PhoneNumberType.Name))
            .ForMember(x => x.ModifiedDate, o => o.MapFrom(y => y.ModifiedDate));
    }
}
