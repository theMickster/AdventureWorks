using AdventureWorks.Domain.Entities.AccountInfo;
using AdventureWorks.Domain.Models.AccountInfo;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles;

public sealed class UserAccountEntityToModelProfile : Profile
{
    public UserAccountEntityToModelProfile()
    {
        CreateMap<UserAccountEntity, UserAccountModel>()
            .ForPath(x => x.Id,
                o => o.MapFrom(y => y.BusinessEntityId))

            .ForPath(x => x.UserName,
                o => o.MapFrom(y => y.UserName))

            .ForPath(x => x.FirstName,
                o => o.MapFrom(y => y.Person.FirstName))

            .ForPath(x => x.MiddleName,
                o => o.MapFrom(y => y.Person.MiddleName))

            .ForPath(x => x.LastName,
                o => o.MapFrom(y => y.Person.LastName))

            .ForPath(x => x.PasswordHash,
                o => o.MapFrom(y => y.PasswordHash));
    }
}
