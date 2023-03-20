using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Models.Sales;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles.Sales;

public sealed class BusinessEntityContactEntityToStoreContactModelProfile : Profile
{
    public BusinessEntityContactEntityToStoreContactModelProfile()
    {
        CreateMap<BusinessEntityContactEntity, StoreContactModel>()
            .ForMember(x => x.Id, o => o.MapFrom(y => y.BusinessEntityId))

            .ForMember(x => x.ContactTypeId, o => o.MapFrom(y => y.ContactTypeId))

            .ForMember(x => x.ContactTypeName, o => o.MapFrom(y => y.ContactType.Name))

            .ForMember(x => x.Title, o => o.MapFrom(y => y.Person.Title))

            .ForMember(x => x.FirstName, o => o.MapFrom(y => y.Person.FirstName))

            .ForMember(x => x.MiddleName, o => o.MapFrom(y => y.Person.MiddleName))

            .ForMember(x => x.LastName, o => o.MapFrom(y => y.Person.LastName))

            .ForMember(x => x.Suffix, o => o.MapFrom(y => y.Person.Suffix));
    }
}
