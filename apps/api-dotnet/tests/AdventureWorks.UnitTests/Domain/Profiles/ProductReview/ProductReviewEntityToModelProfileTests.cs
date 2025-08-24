using AdventureWorks.Application.Features.ProductReview.Profiles;
using AdventureWorks.Models.Features.ProductReview;

namespace AdventureWorks.UnitTests.Domain.Profiles.ProductReview;

[ExcludeFromCodeCoverage]
public sealed class ProductReviewEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public ProductReviewEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ProductReviewEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entity_to_model_succeeds()
    {
        var entity = new AdventureWorks.Domain.Entities.Production.ProductReview
        {
            ProductReviewId = 42,
            ProductId = 937,
            ReviewerName = "John Smith",
            ReviewDate = new DateTime(2024, 6, 15),
            EmailAddress = "john.smith@example.com",
            Rating = 5,
            Comments = "Excellent product!",
            ModifiedDate = DefaultAuditDate
        };

        var result = _mapper.Map<ProductReviewModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ProductReviewId.Should().Be(42);
            result.ProductId.Should().Be(937);
            result.ReviewerName.Should().Be("John Smith");
            result.ReviewDate.Should().Be(new DateTime(2024, 6, 15));
            result.EmailAddress.Should().Be("john.smith@example.com");
            result.Rating.Should().Be(5);
            result.Comments.Should().Be("Excellent product!");
            result.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }
}
