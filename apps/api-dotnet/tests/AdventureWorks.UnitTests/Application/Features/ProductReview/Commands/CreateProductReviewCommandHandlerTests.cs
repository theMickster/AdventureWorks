using AdventureWorks.Application.Features.ProductReview.Commands;
using AdventureWorks.Application.Features.ProductReview.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.ProductReview.Commands;

[ExcludeFromCodeCoverage]
public sealed class CreateProductReviewCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductReviewRepository> _mockProductReviewRepository = new();
    private readonly Mock<IValidator<ProductReviewCreateModel>> _mockValidator = new();
    private CreateProductReviewCommandHandler _sut;

    public CreateProductReviewCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(ProductReviewCreateModelToEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateProductReviewCommandHandler(_mapper, _mockProductReviewRepository.Object, _mockValidator.Object);
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
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateProductReviewCommandHandler(
                    null!,
                    _mockProductReviewRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _ = new CreateProductReviewCommandHandler(
                    _mapper,
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("productReviewRepository");

            _ = ((Action)(() => _ = new CreateProductReviewCommandHandler(
                    _mapper,
                    _mockProductReviewRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_null_Async()
    {
        Func<Task> act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_when_request_model_is_null_Async()
    {
        var command = new CreateProductReviewCommand
        {
            Model = null!,
            ModifiedDate = DefaultAuditDate,
            ReviewDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_validation_fails_Async()
    {
        var command = new CreateProductReviewCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate,
            ReviewDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<ProductReviewCreateModel>("ReviewerName", "Reviewer name is required");
        _sut = new CreateProductReviewCommandHandler(_mapper, _mockProductReviewRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Reviewer name is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_new_product_review_id_on_success_Async()
    {
        const int newReviewId = 42;

        var command = new CreateProductReviewCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate,
            ReviewDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ProductReviewCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockProductReviewRepository.Setup(x => x.AddAsync(
                It.IsAny<AdventureWorks.Domain.Entities.Production.ProductReview>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdventureWorks.Domain.Entities.Production.ProductReview { ProductReviewId = newReviewId });

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(newReviewId);
            _mockProductReviewRepository.Verify(
                x => x.AddAsync(It.IsAny<AdventureWorks.Domain.Entities.Production.ProductReview>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
