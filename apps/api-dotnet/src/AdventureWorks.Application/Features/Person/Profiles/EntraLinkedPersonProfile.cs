using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;

namespace AdventureWorks.Application.Features.Person.Profiles;

/// <summary>
/// AutoMapper profile for mapping PersonEntity to EntraLinkedPersonModel.
/// </summary>
public sealed class EntraLinkedPersonProfile : Profile
{
    public EntraLinkedPersonProfile()
    {
        CreateMap<PersonEntity, EntraLinkedPersonModel>()
            .ForMember(dest => dest.BusinessEntityId, 
                opt => opt.MapFrom(src => src.BusinessEntityId))
            .ForMember(dest => dest.EntraObjectId, 
                opt => opt.MapFrom(src => src.BusinessEntity.Rowguid))
            .ForMember(dest => dest.FirstName, 
                opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, 
                opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.MiddleName, 
                opt => opt.MapFrom(src => src.MiddleName))
            .ForMember(dest => dest.Title, 
                opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.EmailAddress, 
                opt => opt.MapFrom(src => src.EmailAddresses.FirstOrDefault() != null 
                    ? src.EmailAddresses.FirstOrDefault()!.EmailAddressName 
                    : null))
            .ForMember(dest => dest.PersonTypeId, 
                opt => opt.MapFrom(src => src.PersonTypeId))
            .ForMember(dest => dest.PersonTypeName, 
                opt => opt.MapFrom(src => src.PersonType.PersonTypeName))
            .ForMember(dest => dest.IsEntraUser, 
                opt => opt.MapFrom(src => src.BusinessEntity.IsEntraUser));
    }
}
