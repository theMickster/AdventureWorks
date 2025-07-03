using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

/// <summary>
/// AutoMapper profile for mapping EmployeeCreateModel to EmployeeEntity.
/// </summary>
public sealed class EmployeeCreateModelToEmployeeEntityProfile : Profile
{
    public EmployeeCreateModelToEmployeeEntityProfile()
    {
        CreateMap<EmployeeCreateModel, EmployeeEntity>()
            // Map employee-specific fields
            .ForMember(dest => dest.NationalIdnumber,
                opt => opt.MapFrom(src => src.NationalIdNumber))
            .ForMember(dest => dest.LoginId,
                opt => opt.MapFrom(src => src.LoginId))
            .ForMember(dest => dest.JobTitle,
                opt => opt.MapFrom(src => src.JobTitle))
            .ForMember(dest => dest.BirthDate,
                opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.MaritalStatus,
                opt => opt.MapFrom(src => src.MaritalStatus))
            .ForMember(dest => dest.Gender,
                opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.HireDate,
                opt => opt.MapFrom(src => src.HireDate))
            .ForMember(dest => dest.SalariedFlag,
                opt => opt.MapFrom(src => src.SalariedFlag))
            .ForMember(dest => dest.OrganizationLevel,
                opt => opt.MapFrom(src => src.OrganizationLevel))

            // Ignore system-generated fields (set by repository)
            .ForMember(dest => dest.BusinessEntityId,
                opt => opt.Ignore())
            .ForMember(dest => dest.CurrentFlag,
                opt => opt.Ignore())
            .ForMember(dest => dest.VacationHours,
                opt => opt.Ignore())
            .ForMember(dest => dest.SickLeaveHours,
                opt => opt.Ignore())
            .ForMember(dest => dest.Rowguid,
                opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedDate,
                opt => opt.Ignore())

            // Ignore navigation properties
            .ForMember(dest => dest.EmployeeDepartmentHistory,
                opt => opt.Ignore())
            .ForMember(dest => dest.EmployeePayHistory,
                opt => opt.Ignore())
            .ForMember(dest => dest.JobCandidates,
                opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrderHeaders,
                opt => opt.Ignore())
            .ForMember(dest => dest.SalesPersons,
                opt => opt.Ignore())
            .ForMember(dest => dest.PersonBusinessEntity,
                opt => opt.Ignore());
    }
}
