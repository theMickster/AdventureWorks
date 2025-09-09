using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

public sealed class PersonEmailCreateModelToEmailAddressEntityProfile : Profile
{
    public PersonEmailCreateModelToEmailAddressEntityProfile()
    {
        CreateMap<PersonEmailCreateModel, EmailAddressEntity>()
            .ForMember(x => x.EmailAddressName, o => o.MapFrom(y => y.EmailAddress))
            .ForMember(x => x.BusinessEntityId, o => o.Ignore())
            .ForMember(x => x.EmailAddressId, o => o.Ignore())
            .ForMember(x => x.Rowguid, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.BusinessEntity, o => o.Ignore());
    }
}
