using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StorePerformanceProjectionToStorePerformanceModelProfile : Profile
{
    public StorePerformanceProjectionToStorePerformanceModelProfile()
    {
        CreateMap<StorePerformanceProjection, StorePerformanceModel>()
            .ForMember(x => x.StoreId, o => o.MapFrom(y => y.BusinessEntityId))

            .ForMember(x => x.StoreName, o => o.MapFrom(y => y.Name))

            // AverageOrderValue is computed in the handler (RevenueYtd / OrderCount). Ignored here to keep the projection -> model contract clean.
            .ForMember(x => x.AverageOrderValue, o => o.Ignore());
    }
}
