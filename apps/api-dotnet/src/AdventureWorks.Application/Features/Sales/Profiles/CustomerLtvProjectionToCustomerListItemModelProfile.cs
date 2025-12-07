using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class CustomerLtvProjectionToCustomerListItemModelProfile : Profile
{
    public CustomerLtvProjectionToCustomerListItemModelProfile()
    {
        CreateMap<CustomerLtvProjection, CustomerListItemModel>();
    }
}
