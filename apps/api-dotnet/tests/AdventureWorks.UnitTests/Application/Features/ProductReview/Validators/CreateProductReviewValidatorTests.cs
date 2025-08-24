using AdventureWorks.Application.Features.ProductReview.Validators;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.ProductReview;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.ProductReview.Validators;

[ExcludeFromCodeCoverage]
public sealed class CreateProductReviewValidatorTests : UnitTestBase
{
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private CreateProductReviewValidator _sut;

    public CreateProductReviewValidatorTests()
    {
        _mockProductRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new CreateProductReviewValidator(_mockProductRepository.Object);
    }

    private static ProductReviewCreateModel GetValidModel() => new()
    {
        ProductId = 937,
        ReviewerName = "Alice",
        EmailAddress = "alice@example.com",
        Rating = 5,
        Comments = "Great product!"
    };

    [Fact]
    public async Task Validator_passes_for_valid_model_Async()
    {
        var result = await _sut.TestValidateAsync(GetValidModel());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validator_fails_Rule01_when_reviewer_name_is_empty_Async()
    {
        var model = GetValidModel();
        model.ReviewerName = "";

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.ReviewerName).WithErrorCode("Rule-01");
    }

    [Fact]
    public async Task Validator_fails_Rule02_when_reviewer_name_exceeds_50_chars_Async()
    {
        var model = GetValidModel();
        model.ReviewerName = new string('A', 51);

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.ReviewerName).WithErrorCode("Rule-02");
    }

    [Fact]
    public async Task Validator_fails_Rule03_when_email_is_empty_Async()
    {
        var model = GetValidModel();
        model.EmailAddress = "";

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-03");
    }

    [Fact]
    public async Task Validator_fails_Rule04_when_email_format_is_invalid_Async()
    {
        var model = GetValidModel();
        model.EmailAddress = "not-an-email";

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-04");
    }

    [Fact]
    public async Task Validator_fails_Rule05_when_email_exceeds_50_chars_Async()
    {
        var model = GetValidModel();
        model.EmailAddress = new string('a', 45) + "@b.com"; // 51 chars

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.EmailAddress).WithErrorCode("Rule-05");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Validator_fails_Rule06_when_rating_is_out_of_range_Async(int rating)
    {
        var model = GetValidModel();
        model.Rating = rating;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Rating).WithErrorCode("Rule-06");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Validator_passes_for_boundary_ratings_Async(int rating)
    {
        var model = GetValidModel();
        model.Rating = rating;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public async Task Validator_fails_Rule07_when_comments_exceed_3850_chars_Async()
    {
        var model = GetValidModel();
        model.Comments = new string('x', 3851);

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.Comments).WithErrorCode("Rule-07");
    }

    [Fact]
    public async Task Validator_fails_Rule08_when_product_id_is_zero_Async()
    {
        var model = GetValidModel();
        model.ProductId = 0;

        var result = await _sut.TestValidateAsync(model);
        result.ShouldHaveValidationErrorFor(x => x.ProductId).WithErrorCode("Rule-08");
    }

    [Fact]
    public async Task Validator_fails_Rule09_when_product_does_not_exist_Async()
    {
        _mockProductRepository.Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new CreateProductReviewValidator(_mockProductRepository.Object);

        var result = await _sut.TestValidateAsync(GetValidModel());
        result.ShouldHaveValidationErrorFor(x => x.ProductId).WithErrorCode("Rule-09");
    }

    [Fact]
    public async Task Validator_passes_Rule09_when_product_exists_Async()
    {
        _mockProductRepository.Setup(x => x.ExistsAsync(937, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new CreateProductReviewValidator(_mockProductRepository.Object);

        var result = await _sut.TestValidateAsync(GetValidModel());
        result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
    }
}
