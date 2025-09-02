using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

public sealed class StoreSalesPersonHistoryEntityToStoreSalesPersonAssignmentModelProfile : Profile
{
    public StoreSalesPersonHistoryEntityToStoreSalesPersonAssignmentModelProfile()
    {
        CreateMap<StoreSalesPersonHistoryEntity, StoreSalesPersonAssignmentModel>()
            .ForMember(dest => dest.SalesPersonName, opt => opt.MapFrom(src =>
                (src.SalesPerson != null && src.SalesPerson.Employee != null && src.SalesPerson.Employee.PersonBusinessEntity != null)
                    ? src.SalesPerson.Employee.PersonBusinessEntity.FirstName + " " + src.SalesPerson.Employee.PersonBusinessEntity.LastName
                    : string.Empty))
            .ForMember(dest => dest.Territory, opt => opt.MapFrom(src =>
                src.SalesPerson != null && src.SalesPerson.SalesTerritory != null
                    ? src.SalesPerson.SalesTerritory.Name
                    : string.Empty));
    }
}
