using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

public sealed class DepartmentCreateModelToDepartmentEntityProfile : Profile
{
    public DepartmentCreateModelToDepartmentEntityProfile()
    {
        CreateMap<DepartmentCreateModel, DepartmentEntity>()
            .ForPath(d => d.Name, o => o.MapFrom(x => x.Name))
            .ForPath(d => d.GroupName, o => o.MapFrom(x => x.GroupName))
            .ForPath(d => d.ModifiedDate, o => o.Ignore())
            .ForPath(d => d.DepartmentId, o => o.Ignore())
            .ForPath(d => d.EmployeeDepartmentHistory, o => o.Ignore());
    }
}
