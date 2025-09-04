using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

public sealed class EmployeeDepartmentHistoryEntityToModelProfile : Profile
{
    public EmployeeDepartmentHistoryEntityToModelProfile()
    {
        CreateMap<EmployeeDepartmentHistory, EmployeeDepartmentHistoryModel>()
            .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department != null ? s.Department.Name : string.Empty))
            .ForMember(d => d.ShiftName, o => o.MapFrom(s => s.Shift != null ? s.Shift.Name : string.Empty));
    }
}
