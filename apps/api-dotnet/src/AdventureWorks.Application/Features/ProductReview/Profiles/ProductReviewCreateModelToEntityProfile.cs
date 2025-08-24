using AdventureWorks.Models.Features.ProductReview;
using AutoMapper;

namespace AdventureWorks.Application.Features.ProductReview.Profiles;

/// <summary>
/// AutoMapper profile for mapping ProductReviewCreateModel to ProductReview entity.
/// </summary>
public sealed class ProductReviewCreateModelToEntityProfile : Profile
{
    public ProductReviewCreateModelToEntityProfile()
    {
        CreateMap<ProductReviewCreateModel, Domain.Entities.Production.ProductReview>()
            .ForMember(x => x.ProductId, o => o.MapFrom(y => y.ProductId))
            .ForMember(x => x.ReviewerName, o => o.MapFrom(y => y.ReviewerName))
            .ForMember(x => x.EmailAddress, o => o.MapFrom(y => y.EmailAddress))
            .ForMember(x => x.Rating, o => o.MapFrom(y => y.Rating))
            .ForMember(x => x.Comments, o => o.MapFrom(y => y.Comments))
            .ForMember(x => x.ProductReviewId, o => o.Ignore())
            .ForMember(x => x.ReviewDate, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.Product, o => o.Ignore());
    }
}
