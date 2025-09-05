using AdventureWorks.Common.Constants;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;

namespace AdventureWorks.Application.Features.HumanResources.Profiles;

public sealed class EmployeePayHistoryEntityToModelProfile : Profile
{
    public EmployeePayHistoryEntityToModelProfile()
    {
        CreateMap<EmployeePayHistory, EmployeePayHistoryModel>()
            .ForMember(
                d => d.PayFrequencyLabel,
                o => o.MapFrom((s, _) => s.PayFrequency switch
                {
                    HumanResourcesConstants.PayFrequencyMonthly  => "Monthly",
                    HumanResourcesConstants.PayFrequencyBiWeekly => "Bi-Weekly",
                    _ => $"Unknown ({s.PayFrequency})"
                }));
    }
}
