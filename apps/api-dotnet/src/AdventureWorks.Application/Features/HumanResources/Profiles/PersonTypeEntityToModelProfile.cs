using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

public sealed class PersonTypeEntityToModelProfile : Profile
{
    public PersonTypeEntityToModelProfile()
    {
        CreateMap<PersonTypeEntity, PersonTypeModel>()
            .ForPath(a => a.Id,
                o => o.MapFrom(b => b.PersonTypeId))

            .ForPath(a => a.Code,
                o => o.MapFrom(b => b.PersonTypeCode))

            .ForPath(a => a.Description,
                o => o.MapFrom(b => b.PersonTypeDescription))

            .ForPath(a => a.Name,
                o => o.MapFrom(b => b.PersonTypeName));
    }
}
