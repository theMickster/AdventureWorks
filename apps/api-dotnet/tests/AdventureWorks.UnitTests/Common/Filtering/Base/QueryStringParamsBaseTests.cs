using AdventureWorks.Common.Filtering;

namespace AdventureWorks.UnitTests.Common.Filtering.Base;

[ExcludeFromCodeCoverage]
public sealed class QueryStringParamsBaseTests : UnitTestBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void PageSize_clamps_to_minimum_1_when_value_is_zero_or_negative(int invalidPageSize)
    {
        var param = new SalesOrderParameter { PageSize = invalidPageSize };

        param.PageSize.Should().Be(1);
    }

    [Fact]
    public void PageSize_clamps_to_maximum_50_when_value_exceeds_max()
    {
        var param = new SalesOrderParameter { PageSize = 500 };

        param.PageSize.Should().Be(50);
    }

    [Fact]
    public void PageSize_preserves_valid_value_within_bounds()
    {
        var param = new SalesOrderParameter { PageSize = 25 };

        param.PageSize.Should().Be(25);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PageNumber_clamps_to_minimum_1_when_value_is_zero_or_negative(int invalidPageNumber)
    {
        var param = new SalesOrderParameter { PageNumber = invalidPageNumber };

        param.PageNumber.Should().Be(1);
    }

    [Fact]
    public void GetRecordsToSkip_returns_zero_for_page_one()
    {
        var param = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };

        param.GetRecordsToSkip().Should().Be(0);
    }

    [Fact]
    public void GetRecordsToSkip_returns_correct_offset_for_page_two()
    {
        var param = new SalesOrderParameter { PageNumber = 2, PageSize = 10 };

        param.GetRecordsToSkip().Should().Be(10);
    }
}
