using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StoreCustomerProjectionToStoreCustomerModelProfile : Profile
{
    public StoreCustomerProjectionToStoreCustomerModelProfile()
    {
        CreateMap<StoreCustomerProjection, StoreCustomerModel>();
    }
}
