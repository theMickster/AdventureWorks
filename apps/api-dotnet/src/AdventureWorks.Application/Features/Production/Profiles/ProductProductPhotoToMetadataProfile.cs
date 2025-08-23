using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

public sealed class ProductProductPhotoToMetadataProfile : Profile
{
    public ProductProductPhotoToMetadataProfile()
    {
        CreateMap<ProductProductPhoto, ProductPhotoMetadataModel>()
            .ForMember(d => d.PhotoId, o => o.MapFrom(s => s.ProductPhotoId))
            .ForMember(d => d.ThumbnailPhotoFileName, o => o.MapFrom(s => s.ProductPhoto != null ? s.ProductPhoto.ThumbnailPhotoFileName : null))
            .ForMember(d => d.LargePhotoFileName, o => o.MapFrom(s => s.ProductPhoto != null ? s.ProductPhoto.LargePhotoFileName : null))
            .ForMember(d => d.IsPrimary, o => o.MapFrom(s => s.Primary));
    }
}
