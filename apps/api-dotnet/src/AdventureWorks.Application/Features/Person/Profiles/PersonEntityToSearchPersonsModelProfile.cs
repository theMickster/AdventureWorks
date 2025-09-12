using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

/// <summary>
/// Maps Person entities to search result models.
/// </summary>
public sealed class PersonEntityToSearchPersonsModelProfile : Profile
{
    public PersonEntityToSearchPersonsModelProfile()
    {
        CreateMap<PersonEntity, SearchPersonsModel>()
            .ForMember(dest => dest.BusinessEntityId, opt => opt.MapFrom(src => src.BusinessEntityId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.PersonTypeName, opt => opt.MapFrom(src => src.PersonType != null ? src.PersonType.PersonTypeName : string.Empty))
            .ForMember(dest => dest.PrimaryEmail, opt => opt.MapFrom(src => src.EmailAddresses != null && src.EmailAddresses.Any() ? src.EmailAddresses.First().EmailAddressName : null));
    }
}
