using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

public sealed class DepartmentEntityToDepartmentModelProfile : Profile
{
    public DepartmentEntityToDepartmentModelProfile()
    {
        CreateMap<DepartmentEntity, DepartmentModel>()
            .ForPath(a => a.Id,
                o => o.MapFrom(x => x.DepartmentId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.GroupName,
                o => o.MapFrom(x => x.GroupName))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
