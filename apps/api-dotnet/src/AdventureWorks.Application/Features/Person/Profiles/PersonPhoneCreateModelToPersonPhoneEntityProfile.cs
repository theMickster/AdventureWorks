using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

public sealed class PersonPhoneCreateModelToPersonPhoneEntityProfile : Profile
{
    public PersonPhoneCreateModelToPersonPhoneEntityProfile()
    {
        CreateMap<PersonPhoneCreateModel, PersonPhone>()
            .ForMember(x => x.PhoneNumber, o => o.MapFrom(y => y.PhoneNumber))
            .ForMember(x => x.PhoneNumberTypeId, o => o.MapFrom(y => y.PhoneNumberTypeId))
            .ForMember(x => x.BusinessEntityId, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.BusinessEntity, o => o.Ignore())
            .ForMember(x => x.PhoneNumberType, o => o.Ignore());
    }
}
