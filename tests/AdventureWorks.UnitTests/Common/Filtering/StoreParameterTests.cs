using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
#pragma warning disable CS8601 // Possible null reference assignment.

namespace AdventureWorks.UnitTests.Common.Filtering;

[ExcludeFromCodeCoverage]
public sealed class StoreParameterTests : UnitTestBase
{
    [Fact]
    public void MaxTake_value_is_not_exceeded()
    {
        var param = new StoreParameter
        {
            PageNumber = 1,
            OrderBy = "id",
            PageSize = 500
        };

        param.PageSize.Should().Be(50);
    }

    [Fact]
    public void MinPageNumber_value_is_not_exceeded()
    {
        var param = new StoreParameter
        {
            PageNumber = 0,
            OrderBy = "name",
            PageSize = 10
        };

        param.PageSize.Should().Be(10);
        param.PageNumber.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 50, 100)]
    [InlineData(6, 25, 125)]
    public void Skip_is_properly_calculated(int pageNumber, int pageSize, int expectedSkip)
    {
        var param = new StoreParameter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        param.GetRecordsToSkip().Should().Be(expectedSkip);
    }

    [Theory]
    [InlineData("ID", "BusinessEntityId")]
    [InlineData("Id", "BusinessEntityId")]
    [InlineData("iD", "BusinessEntityId")]
    [InlineData("StoreId", "BusinessEntityId")]
    [InlineData("name", "Name")]
    [InlineData("NaMe", "Name")]
    [InlineData("NAME", "Name")]
    [InlineData("StoreName", "BusinessEntityId")]
    [InlineData(null, "BusinessEntityId")]
    public void OrderBy_field_is_calculated_correctly(string? orderByInput, string orderByExpectedOutput)
    {
        var param = new StoreParameter
        {
            OrderBy = orderByInput
        };
        param.OrderBy.Should().Be(orderByExpectedOutput);
    }

    [Fact]
    public void OrderByDefault_is_correct()
    {
        var param = new StoreParameter();
        param.OrderBy.Should().Be("BusinessEntityId");
    }

    [Theory]
    [InlineData(null, SortedResultConstants.Ascending)]
    [InlineData("", SortedResultConstants.Ascending)]
    [InlineData("asc", SortedResultConstants.Ascending)]
    [InlineData("ascending ", SortedResultConstants.Ascending)]
    [InlineData(" ASCENDING", SortedResultConstants.Ascending)]
    [InlineData("desc", SortedResultConstants.Descending)]
    [InlineData("descending ", SortedResultConstants.Descending)]
    [InlineData(" DESCENDING", SortedResultConstants.Descending)]
    public void SortOrder_field_is_calculated_correctly(string? sortInput, string sortExpectedOutput)
    {
        var param = new StoreParameter
        {
            SortOrder = sortInput
        };
        param.SortOrder.Should().Be(sortExpectedOutput);
    }
}
