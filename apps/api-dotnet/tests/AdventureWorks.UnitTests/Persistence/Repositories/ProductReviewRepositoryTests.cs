using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class ProductReviewRepositoryTests : PersistenceUnitTestBase
{
    private readonly ProductReviewRepository _sut;

    public ProductReviewRepositoryTests()
    {
        _sut = new ProductReviewRepository(DbContext);
        LoadMockProductReviews();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ProductReviewRepository)
                .Should().Implement<IProductReviewRepository>();

            typeof(ProductReviewRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetProductReviewsByProductIdAsync_returns_reviews_for_product()
    {
        var parameters = new ProductReviewParameter { PageNumber = 1, PageSize = 10 };

        var (results, totalCount) = await _sut.GetProductReviewsByProductIdAsync(937, parameters);

        using (new AssertionScope())
        {
            results.Should().NotBeNull();
            results.Should().HaveCount(3);
            totalCount.Should().Be(3);
        }
    }

    [Fact]
    public async Task GetProductReviewsByProductIdAsync_returns_empty_for_no_reviews()
    {
        var parameters = new ProductReviewParameter { PageNumber = 1, PageSize = 10 };

        var (results, totalCount) = await _sut.GetProductReviewsByProductIdAsync(9999, parameters);

        using (new AssertionScope())
        {
            results.Should().NotBeNull();
            results.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetProductReviewsByProductIdAsync_sorts_by_rating_descending()
    {
        var parameters = new ProductReviewParameter { PageNumber = 1, PageSize = 10, OrderBy = "rating", SortOrder = "desc" };

        var (results, _) = await _sut.GetProductReviewsByProductIdAsync(937, parameters);

        using (new AssertionScope())
        {
            results.Should().HaveCount(3);
            results[0].Rating.Should().Be(5);
            results[1].Rating.Should().Be(3);
            results[2].Rating.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetProductReviewsByProductIdAsync_paginates_correctly()
    {
        var parameters = new ProductReviewParameter { PageNumber = 1, PageSize = 2 };

        var (results, totalCount) = await _sut.GetProductReviewsByProductIdAsync(937, parameters);

        using (new AssertionScope())
        {
            results.Should().HaveCount(2);
            totalCount.Should().Be(3);
        }
    }

    [Fact]
    public async Task GetRatingDistributionByProductIdAsync_returns_empty_dictionary_when_no_reviews_exist_Async()
    {
        var result = await _sut.GetRatingDistributionByProductIdAsync(9999);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRatingDistributionByProductIdAsync_returns_correct_counts_per_rating_Async()
    {
        // Seed 5 extra reviews: ratings 5, 5, 3, 1, 5 for product 100
        DbContext.ProductReviews.AddRange(new List<ProductReview>
        {
            new() { ProductReviewId = 10, ProductId = 100, ReviewerName = "U1", ReviewDate = StandardModifiedDate, EmailAddress = "u1@test.com", Rating = 5, ModifiedDate = StandardModifiedDate },
            new() { ProductReviewId = 11, ProductId = 100, ReviewerName = "U2", ReviewDate = StandardModifiedDate, EmailAddress = "u2@test.com", Rating = 5, ModifiedDate = StandardModifiedDate },
            new() { ProductReviewId = 12, ProductId = 100, ReviewerName = "U3", ReviewDate = StandardModifiedDate, EmailAddress = "u3@test.com", Rating = 3, ModifiedDate = StandardModifiedDate },
            new() { ProductReviewId = 13, ProductId = 100, ReviewerName = "U4", ReviewDate = StandardModifiedDate, EmailAddress = "u4@test.com", Rating = 1, ModifiedDate = StandardModifiedDate },
            new() { ProductReviewId = 14, ProductId = 100, ReviewerName = "U5", ReviewDate = StandardModifiedDate, EmailAddress = "u5@test.com", Rating = 5, ModifiedDate = StandardModifiedDate }
        });
        await DbContext.SaveChangesAsync();

        var result = await _sut.GetRatingDistributionByProductIdAsync(100);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result[5].Should().Be(3);
            result[3].Should().Be(1);
            result[1].Should().Be(1);
        }
    }

    [Fact]
    public async Task GetRatingDistributionByProductIdAsync_excludes_reviews_for_other_products_Async()
    {
        DbContext.ProductReviews.AddRange(new List<ProductReview>
        {
            new() { ProductReviewId = 20, ProductId = 937, ReviewerName = "P1", ReviewDate = StandardModifiedDate, EmailAddress = "p1@test.com", Rating = 4, ModifiedDate = StandardModifiedDate },
            new() { ProductReviewId = 21, ProductId = 938, ReviewerName = "P2", ReviewDate = StandardModifiedDate, EmailAddress = "p2@test.com", Rating = 2, ModifiedDate = StandardModifiedDate }
        });
        await DbContext.SaveChangesAsync();

        var result = await _sut.GetRatingDistributionByProductIdAsync(937);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().NotContainKey(2);
            result.Values.Should().AllSatisfy(v => v.Should().BeGreaterThan(0));
        }
    }

    private void LoadMockProductReviews()
    {
        DbContext.ProductReviews.AddRange(new List<ProductReview>
        {
            new()
            {
                ProductReviewId = 1, ProductId = 937, ReviewerName = "Alice",
                ReviewDate = new DateTime(2024, 1, 10), EmailAddress = "alice@example.com",
                Rating = 5, Comments = "Great!", ModifiedDate = StandardModifiedDate
            },
            new()
            {
                ProductReviewId = 2, ProductId = 937, ReviewerName = "Bob",
                ReviewDate = new DateTime(2024, 2, 15), EmailAddress = "bob@example.com",
                Rating = 3, Comments = "Decent.", ModifiedDate = StandardModifiedDate
            },
            new()
            {
                ProductReviewId = 3, ProductId = 937, ReviewerName = "Charlie",
                ReviewDate = new DateTime(2024, 3, 20), EmailAddress = "charlie@example.com",
                Rating = 1, Comments = "Not great.", ModifiedDate = StandardModifiedDate
            },
            new()
            {
                ProductReviewId = 4, ProductId = 798, ReviewerName = "Diana",
                ReviewDate = new DateTime(2024, 4, 25), EmailAddress = "diana@example.com",
                Rating = 4, Comments = "Pretty good.", ModifiedDate = StandardModifiedDate
            }
        });

        DbContext.SaveChanges();
    }
}
