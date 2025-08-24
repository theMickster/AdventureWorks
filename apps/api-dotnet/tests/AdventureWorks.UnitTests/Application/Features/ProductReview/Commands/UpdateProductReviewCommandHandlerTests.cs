using AdventureWorks.Application.Features.ProductReview.Commands;
using AdventureWorks.Application.Features.ProductReview.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.ProductReview;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.ProductReview.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdateProductReviewCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductReviewRepository> _mockProductReviewRepository = new();
    private readonly Mock<IValidator<ProductReviewUpdateModel>> _mockValidator = new();
    private UpdateProductReviewCommandHandler _sut;

    public UpdateProductReviewCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(ProductReviewUpdateModelToEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new UpdateProductReviewCommandHandler(_mapper, _mockProductReviewRepository.Object, _mockValidator.Object);
    }

    private static ProductReviewUpdateModel GetValidModel() => new()
    {
        Id = 1,
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
            _ = ((Action)(() => _ = new UpdateProductReviewCommandHandler(
                    null!,
                    _mockProductReviewRepository.Object,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _ = new UpdateProductReviewCommandHandler(
                    _mapper,
                    null!,
                    _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("productReviewRepository");

            _ = ((Action)(() => _ = new UpdateProductReviewCommandHandler(
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
        var command = new UpdateProductReviewCommand
        {
            Model = null!,
            ModifiedDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_validation_fails_Async()
    {
        var command = new UpdateProductReviewCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<ProductReviewUpdateModel>("ReviewerName", "Reviewer name is required");
        _sut = new UpdateProductReviewCommandHandler(_mapper, _mockProductReviewRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Reviewer name is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_when_entity_does_not_exist_Async()
    {
        var command = new UpdateProductReviewCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ProductReviewUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockProductReviewRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdventureWorks.Domain.Entities.Production.ProductReview?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{GetValidModel().Id}*");
    }

    [Fact]
    public async Task Handle_calls_update_once_and_returns_unit_on_success_Async()
    {
        var command = new UpdateProductReviewCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate
        };

        var existingEntity = new AdventureWorks.Domain.Entities.Production.ProductReview
        {
            ProductReviewId = 1,
            ProductId = 937,
            ReviewerName = "Bob",
            EmailAddress = "bob@example.com",
            Rating = 3,
            ReviewDate = DefaultAuditDate,
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ProductReviewUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockProductReviewRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        _mockProductReviewRepository.Setup(x => x.UpdateAsync(
                It.IsAny<AdventureWorks.Domain.Entities.Production.ProductReview>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockProductReviewRepository.Verify(
            x => x.UpdateAsync(It.IsAny<AdventureWorks.Domain.Entities.Production.ProductReview>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
