using AdventureWorks.Models.Features.Production;
using AutoMapper;
using DomainProductModel = AdventureWorks.Domain.Entities.Production.ProductModel;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductModelToLookupModelsProfile : Profile
{
    public ProductModelToLookupModelsProfile()
    {
        CreateMap<DomainProductModel, ProductModelListModel>()
            .ForPath(a => a.ProductModelId,
                o => o.MapFrom(x => x.ProductModelId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));

        CreateMap<DomainProductModel, ProductModelDetailModel>()
            .ForPath(a => a.ProductModelId,
                o => o.MapFrom(x => x.ProductModelId))

            .ForPath(a => a.Name,
                o => o.MapFrom(x => x.Name))

            .ForPath(a => a.CatalogDescription,
                o => o.MapFrom(x => x.CatalogDescription))

            .ForPath(a => a.ModifiedDate,
                o => o.MapFrom(x => x.ModifiedDate));
    }
}
