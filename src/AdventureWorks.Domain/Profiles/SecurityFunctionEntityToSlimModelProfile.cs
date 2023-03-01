using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;
using AutoMapper;

namespace AdventureWorks.Domain.Profiles;

public sealed class SecurityFunctionEntityToSlimModelProfile : Profile
{
    public SecurityFunctionEntityToSlimModelProfile()
    {
        CreateMap<SecurityFunctionEntity, SecurityFunctionSlimModel>()
            .ForPath(x => x.Id,
                o => o.MapFrom(y => y.Id))

            .ForPath(x => x.Name,
                o => o.MapFrom(y => y.Name))
            
            .ForPath(x => x.Code, 
                o => o.Ignore());
    }
}
