using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

/// <summary>
/// Maps Person entities to consolidated person detail models.
/// </summary>
public sealed class PersonEntityToPersonDetailModelProfile : Profile
{
    public PersonEntityToPersonDetailModelProfile()
    {
        CreateMap<PersonEntity, PersonDetailModel>()
            .ForMember(dest => dest.BusinessEntityId, opt => opt.MapFrom(src => src.BusinessEntityId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.Suffix))
            .ForMember(dest => dest.PersonTypeName, opt => opt.MapFrom(src => src.PersonType != null ? src.PersonType.PersonTypeName : null))
            .ForMember(dest => dest.EmailPromotion, opt => opt.MapFrom(src => src.EmailPromotion))
            .ForMember(dest => dest.EmailAddresses, opt => opt.MapFrom(src => src.EmailAddresses))
            .ForMember(dest => dest.PhoneNumbers, opt => opt.MapFrom(src => src.PersonPhones));
    }
}
