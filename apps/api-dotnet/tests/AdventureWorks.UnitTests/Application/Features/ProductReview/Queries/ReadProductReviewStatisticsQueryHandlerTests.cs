using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;

namespace AdventureWorks.UnitTests.Application.Features.ProductReview.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductReviewStatisticsQueryHandlerTests : UnitTestBase
{
    private readonly Mock<IProductReviewRepository> _mockProductReviewRepository = new();
    private ReadProductReviewStatisticsQueryHandler _sut;

    public ReadProductReviewStatisticsQueryHandlerTests()
    {
        _sut = new ReadProductReviewStatisticsQueryHandler(_mockProductReviewRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exception()
    {
        ((Action)(() => _ = new ReadProductReviewStatisticsQueryHandler(null!)))
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
    public async Task Handle_returns_zero_stats_when_no_reviews_exist_Async()
    {
        _mockProductReviewRepository
            .Setup(x => x.GetRatingDistributionByProductIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyDictionary<int, int>)new Dictionary<int, int>());

        var result = await _sut.Handle(
            new ReadProductReviewStatisticsQuery { ProductId = 937 },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ProductId.Should().Be(937);
            result.TotalReviews.Should().Be(0);
            result.AverageRating.Should().Be(0.0);
            result.RatingDistribution.Should().HaveCount(5);
            result.RatingDistribution.Values.Should().AllSatisfy(v => v.Should().Be(0));
        }
    }

    [Fact]
    public async Task Handle_returns_correct_statistics_Async()
    {
        // ratings: 5, 5, 3, 1, 5  => distribution {1:1, 3:1, 5:3} => avg = 3.8, total = 5
        _mockProductReviewRepository
            .Setup(x => x.GetRatingDistributionByProductIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyDictionary<int, int>)new Dictionary<int, int> { { 1, 1 }, { 3, 1 }, { 5, 3 } });

        var result = await _sut.Handle(
            new ReadProductReviewStatisticsQuery { ProductId = 937 },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.ProductId.Should().Be(937);
            result.TotalReviews.Should().Be(5);
            result.AverageRating.Should().Be(3.8);
            result.RatingDistribution[1].Should().Be(1);
            result.RatingDistribution[2].Should().Be(0);
            result.RatingDistribution[3].Should().Be(1);
            result.RatingDistribution[4].Should().Be(0);
            result.RatingDistribution[5].Should().Be(3);
        }
    }
}
