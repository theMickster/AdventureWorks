using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

/// <summary>
/// AutoMapper profile for mapping EmployeeEntity to EmployeeModel (read operations).
/// </summary>
public sealed class EmployeeEntityToModelProfile : Profile
{
    public EmployeeEntityToModelProfile()
    {
        CreateMap<EmployeeEntity, EmployeeModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BusinessEntityId))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.PersonBusinessEntity.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.PersonBusinessEntity.LastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.PersonBusinessEntity.MiddleName))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.PersonBusinessEntity.Title))
            .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.PersonBusinessEntity.Suffix))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobTitle))
            .ForMember(dest => dest.MaritalStatus, opt => opt.MapFrom(src => src.MaritalStatus))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
            .ForMember(dest => dest.SalariedFlag, opt => opt.MapFrom(src => src.SalariedFlag))
            .ForMember(dest => dest.OrganizationLevel, opt => opt.MapFrom(src => src.OrganizationLevel))
            .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdnumber))
            .ForMember(dest => dest.LoginId, opt => opt.MapFrom(src => src.LoginId))
            .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate))
            .ForMember(dest => dest.CurrentFlag, opt => opt.MapFrom(src => src.CurrentFlag))
            .ForMember(dest => dest.VacationHours, opt => opt.MapFrom(src => src.VacationHours))
            .ForMember(dest => dest.SickLeaveHours, opt => opt.MapFrom(src => src.SickLeaveHours))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom<EmployeeEmailAddressResolver>())
            .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate));
    }
}

/// <summary>
/// Custom resolver to extract the first email address from PersonBusinessEntity.EmailAddresses collection.
/// </summary>
public class EmployeeEmailAddressResolver : IValueResolver<EmployeeEntity, EmployeeModel, string?>
{
    public string? Resolve(EmployeeEntity src, EmployeeModel dest, string? destMember, ResolutionContext context)
    {
        return src.PersonBusinessEntity?.EmailAddresses?.FirstOrDefault()?.EmailAddressName;
    }
}
