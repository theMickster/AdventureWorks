using AdventureWorks.Application.Features.ProductReview.Profiles;
using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;

namespace AdventureWorks.UnitTests.Application.Features.ProductReview.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductReviewQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductReviewRepository> _mockProductReviewRepository = new();
    private ReadProductReviewQueryHandler _sut;

    public ReadProductReviewQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ProductReviewEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadProductReviewQueryHandler(_mapper, _mockProductReviewRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadProductReviewQueryHandler(
                    null!,
                    _mockProductReviewRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadProductReviewQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("productReviewRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_null_Async()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_null_when_entity_not_found_Async()
    {
        _mockProductReviewRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdventureWorks.Domain.Entities.Production.ProductReview?)null);

        var result = await _sut.Handle(new ReadProductReviewQuery { Id = 999 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_mapped_model_when_entity_found_Async()
    {
        var entity = new AdventureWorks.Domain.Entities.Production.ProductReview
        {
            ProductReviewId = 1,
            ProductId = 937,
            ReviewerName = "Alice",
            ReviewDate = new DateTime(2024, 1, 10),
            EmailAddress = "alice@example.com",
            Rating = 5,
            Comments = "Great!",
            ModifiedDate = DefaultAuditDate
        };

        _mockProductReviewRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.Handle(new ReadProductReviewQuery { Id = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.ProductReviewId.Should().Be(1);
            result.ProductId.Should().Be(937);
            result.ReviewerName.Should().Be("Alice");
            result.Rating.Should().Be(5);
        }
    }
}
