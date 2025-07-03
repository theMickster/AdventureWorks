using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class SalesPersonEntityToModelProfile : Profile
{
    public SalesPersonEntityToModelProfile()
    {
        CreateMap<SalesPersonEntity, SalesPersonModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BusinessEntityId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Employee.PersonBusinessEntity.Title))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Employee.PersonBusinessEntity.FirstName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.Employee.PersonBusinessEntity.MiddleName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Employee.PersonBusinessEntity.LastName))
            .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.Employee.PersonBusinessEntity.Suffix))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Employee.JobTitle))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom<EmailAddressResolver>())
            .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate));
    }
}

public class EmailAddressResolver : IValueResolver<SalesPersonEntity, SalesPersonModel, string?>
{
    public string? Resolve(SalesPersonEntity src, SalesPersonModel dest, string? destMember, ResolutionContext context)
    {
        return src.Employee?.PersonBusinessEntity?.EmailAddresses?.FirstOrDefault()?.EmailAddressName;
    }
}