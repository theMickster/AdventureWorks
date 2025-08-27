using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StoreDemographicsProjectionToStoreDemographicsModelProfile : Profile
{
    public StoreDemographicsProjectionToStoreDemographicsModelProfile()
    {
        CreateMap<StoreDemographicsProjection, StoreDemographicsModel>()
            .ForMember(x => x.StoreId, o => o.MapFrom(y => y.BusinessEntityId))

            .ForMember(x => x.StoreName, o => o.MapFrom(y => y.Name))

            .ForMember(x => x.AnnualSales, o => o.Ignore())

            .ForMember(x => x.AnnualRevenue, o => o.Ignore())

            .ForMember(x => x.BankName, o => o.Ignore())

            .ForMember(x => x.BusinessType, o => o.Ignore())

            .ForMember(x => x.YearOpened, o => o.Ignore())

            .ForMember(x => x.Specialty, o => o.Ignore())

            .ForMember(x => x.SquareFeet, o => o.Ignore())

            .ForMember(x => x.Internet, o => o.Ignore())

            .ForMember(x => x.NumberEmployees, o => o.Ignore())

            .ForMember(x => x.Brands, o => o.Ignore());
    }
}
