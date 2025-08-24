using AdventureWorks.Models.Features.ProductReview;
using AutoMapper;

namespace AdventureWorks.Application.Features.ProductReview.Profiles;

/// <summary>
/// AutoMapper profile for mapping ProductReview entity to ProductReviewModel.
/// </summary>
public sealed class ProductReviewEntityToModelProfile : Profile
{
    public ProductReviewEntityToModelProfile()
    {
        CreateMap<Domain.Entities.Production.ProductReview, ProductReviewModel>()
            .ForMember(x => x.ProductReviewId, o => o.MapFrom(y => y.ProductReviewId))
            .ForMember(x => x.ProductId, o => o.MapFrom(y => y.ProductId))
            .ForMember(x => x.ReviewerName, o => o.MapFrom(y => y.ReviewerName))
            .ForMember(x => x.ReviewDate, o => o.MapFrom(y => y.ReviewDate))
            .ForMember(x => x.EmailAddress, o => o.MapFrom(y => y.EmailAddress))
            .ForMember(x => x.Rating, o => o.MapFrom(y => y.Rating))
            .ForMember(x => x.Comments, o => o.MapFrom(y => y.Comments))
            .ForMember(x => x.ModifiedDate, o => o.MapFrom(y => y.ModifiedDate));
    }
}
