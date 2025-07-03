using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

public sealed class ContactTypeEntityToModelProfile : Profile
{
    public ContactTypeEntityToModelProfile()
    {
        CreateMap<ContactTypeEntity, ContactTypeModel>()
            .ForPath(a => a.Id,
                o => o.MapFrom(b => b.ContactTypeId))

            .ForPath(a => a.Name,
                o => o.MapFrom(b => b.Name))

            .ForPath(x => x.Code, o => o.Ignore())

            .ForPath(x => x.Description, o => o.Ignore());
    }
}
