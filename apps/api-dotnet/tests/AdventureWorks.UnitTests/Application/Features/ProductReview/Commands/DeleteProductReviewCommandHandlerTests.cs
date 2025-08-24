using AdventureWorks.Application.Features.ProductReview.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories;

namespace AdventureWorks.UnitTests.Application.Features.ProductReview.Commands;

[ExcludeFromCodeCoverage]
public sealed class DeleteProductReviewCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IProductReviewRepository> _mockProductReviewRepository = new();
    private readonly DeleteProductReviewCommandHandler _sut;

    public DeleteProductReviewCommandHandlerTests()
    {
        _sut = new DeleteProductReviewCommandHandler(_mockProductReviewRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exception()
    {
        _ = ((Action)(() => _ = new DeleteProductReviewCommandHandler(null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("productReviewRepository");
    }

    [Fact]
    public async Task Handle_throws_when_request_is_null_Async()
    {
        Func<Task> act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_key_not_found_when_entity_does_not_exist_Async()
    {
        var command = new DeleteProductReviewCommand { Id = 1 };

        _mockProductReviewRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdventureWorks.Domain.Entities.Production.ProductReview?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.Id}*");
    }

    [Fact]
    public async Task Handle_calls_delete_once_and_returns_unit_on_success_Async()
    {
        var command = new DeleteProductReviewCommand { Id = 1 };

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

        _mockProductReviewRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        _mockProductReviewRepository.Setup(x => x.DeleteAsync(
                It.IsAny<AdventureWorks.Domain.Entities.Production.ProductReview>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockProductReviewRepository.Verify(
            x => x.DeleteAsync(It.IsAny<AdventureWorks.Domain.Entities.Production.ProductReview>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
