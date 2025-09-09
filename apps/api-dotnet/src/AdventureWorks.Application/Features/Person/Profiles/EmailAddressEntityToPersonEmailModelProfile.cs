using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

public sealed class EmailAddressEntityToPersonEmailModelProfile : Profile
{
    public EmailAddressEntityToPersonEmailModelProfile()
    {
        CreateMap<EmailAddressEntity, PersonEmailModel>()
            .ForMember(x => x.EmailAddressId, o => o.MapFrom(y => y.EmailAddressId))
            .ForMember(x => x.EmailAddress, o => o.MapFrom(y => y.EmailAddressName))
            .ForMember(x => x.ModifiedDate, o => o.MapFrom(y => y.ModifiedDate));
    }
}
