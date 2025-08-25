using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StoreContactCreateModelToBusinessEntityContactEntityProfile : Profile
{
    public StoreContactCreateModelToBusinessEntityContactEntityProfile()
    {
        CreateMap<StoreContactCreateModel, BusinessEntityContactEntity>()
            .ForMember(x => x.PersonId, o => o.MapFrom(y => y.PersonId))
            .ForMember(x => x.ContactTypeId, o => o.MapFrom(y => y.ContactTypeId))
            .ForMember(x => x.BusinessEntityId, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.Rowguid, o => o.Ignore())
            .ForMember(x => x.BusinessEntity, o => o.Ignore())
            .ForMember(x => x.ContactType, o => o.Ignore())
            .ForMember(x => x.Person, o => o.Ignore());
    }
}
