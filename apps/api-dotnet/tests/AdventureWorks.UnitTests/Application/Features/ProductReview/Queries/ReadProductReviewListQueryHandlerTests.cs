using AdventureWorks.Application.Features.ProductReview.Profiles;
using AdventureWorks.Application.Features.ProductReview.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;

namespace AdventureWorks.UnitTests.Application.Features.ProductReview.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadProductReviewListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductReviewRepository> _mockProductReviewRepository = new();
    private ReadProductReviewListQueryHandler _sut;

    public ReadProductReviewListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ProductReviewEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadProductReviewListQueryHandler(_mapper, _mockProductReviewRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadProductReviewListQueryHandler(
                    null!,
                    _mockProductReviewRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadProductReviewListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("productReviewRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_correct_null_resultAsync()
    {
        _mockProductReviewRepository.Setup(x => x.GetProductReviewsByProductIdAsync(
                It.IsAny<int>(), It.IsAny<ProductReviewParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((null!, 0));

        var result = await _sut.Handle(
            new ReadProductReviewListQuery { ProductId = 937, Parameters = new ProductReviewParameter() },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_returns_correct_empty_resultAsync()
    {
        var readOnlyList = new List<AdventureWorks.Domain.Entities.Production.ProductReview>().AsReadOnly();
        _mockProductReviewRepository.Setup(x => x.GetProductReviewsByProductIdAsync(
                It.IsAny<int>(), It.IsAny<ProductReviewParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((readOnlyList, 0));

        var result = await _sut.Handle(
            new ReadProductReviewListQuery { ProductId = 937, Parameters = new ProductReviewParameter() },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_returns_valid_paged_model_Async()
    {
        var reviews = new List<AdventureWorks.Domain.Entities.Production.ProductReview>
        {
            new()
            {
                ProductReviewId = 1, ProductId = 937, ReviewerName = "Alice",
                ReviewDate = new DateTime(2024, 1, 10), EmailAddress = "alice@example.com",
                Rating = 5, Comments = "Great!", ModifiedDate = DefaultAuditDate
            },
            new()
            {
                ProductReviewId = 2, ProductId = 937, ReviewerName = "Bob",
                ReviewDate = new DateTime(2024, 2, 15), EmailAddress = "bob@example.com",
                Rating = 3, Comments = "Decent.", ModifiedDate = DefaultAuditDate
            }
        };

        _mockProductReviewRepository.Setup(x => x.GetProductReviewsByProductIdAsync(
                It.IsAny<int>(), It.IsAny<ProductReviewParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((reviews.AsReadOnly(), 2));

        var param = new ProductReviewParameter { PageNumber = 1, OrderBy = "rating", PageSize = 10, SortOrder = "DESCENDING" };

        var pagedResult = await _sut.Handle(
            new ReadProductReviewListQuery { ProductId = 937, Parameters = param },
            CancellationToken.None);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();
            pagedResult.PageNumber.Should().Be(1);
            pagedResult.PageSize.Should().Be(10);
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse();
            pagedResult.TotalPages.Should().Be(1);
            pagedResult.TotalRecords.Should().Be(2);
            pagedResult.Results.Should().HaveCount(2);

            var review01 = pagedResult.Results.FirstOrDefault(x => x.ProductReviewId == 1);
            review01!.Should().NotBeNull();
            review01!.ReviewerName.Should().Be("Alice");
            review01!.Rating.Should().Be(5);
        }
    }
}
