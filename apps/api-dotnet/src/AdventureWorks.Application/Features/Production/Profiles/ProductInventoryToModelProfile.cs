using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductInventoryToModelProfile : Profile
{
    public ProductInventoryToModelProfile()
    {
        CreateMap<ProductInventory, ProductInventoryModel>()
            .ForMember(d => d.LocationName, o => o.MapFrom(s => s.Location != null ? s.Location.Name : string.Empty));
    }
}
