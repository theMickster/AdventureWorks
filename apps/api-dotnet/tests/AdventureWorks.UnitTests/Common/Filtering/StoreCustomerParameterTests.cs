using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
#pragma warning disable CS8601 // Possible null reference assignment.

namespace AdventureWorks.UnitTests.Common.Filtering;

[ExcludeFromCodeCoverage]
public sealed class StoreCustomerParameterTests : UnitTestBase
{
    [Fact]
    public void MaxTake_value_is_not_exceeded()
    {
        var param = new StoreCustomerParameter
        {
            PageNumber = 1,
            PageSize = 500
        };

        param.PageSize.Should().Be(50);
    }

    [Fact]
    public void MinPageNumber_value_is_not_exceeded()
    {
        var param = new StoreCustomerParameter
        {
            PageNumber = 0,
            PageSize = 10
        };

        using (new AssertionScope())
        {
            param.PageNumber.Should().Be(1);
            param.PageSize.Should().Be(10);
        }
    }

    [Theory]
    [InlineData("LifetimeSpend", "LifetimeSpend")]
    [InlineData("lifetimespend", "LifetimeSpend")]
    [InlineData("LIFETIMESPEND", "LifetimeSpend")]
    [InlineData(" LifetimeSpend ", "LifetimeSpend")]
    [InlineData("PersonName", "PersonName")]
    [InlineData("personname", "PersonName")]
    [InlineData("OrderCount", "OrderCount")]
    [InlineData("ordercount", "OrderCount")]
    [InlineData("LastOrderDate", "LastOrderDate")]
    [InlineData("lastorderdate", "LastOrderDate")]
    [InlineData("UnknownField", "LifetimeSpend")]
    [InlineData("", "LifetimeSpend")]
    [InlineData(null, "LifetimeSpend")]
    public void OrderBy_field_is_calculated_correctly(string? orderByInput, string orderByExpectedOutput)
    {
        var param = new StoreCustomerParameter
        {
            OrderBy = orderByInput
        };

        param.OrderBy.Should().Be(orderByExpectedOutput);
    }

    [Fact]
    public void OrderBy_default_is_LifetimeSpend()
    {
        var param = new StoreCustomerParameter();

        param.OrderBy.Should().Be(StoreCustomerParameter.LifetimeSpend);
    }

    [Fact]
    public void SortOrder_default_is_Descending_via_virtual_override()
    {
        // The base QueryStringParamsBase defaults SortOrder to Ascending.
        // StoreCustomerParameter overrides that property so the customer list
        // defaults to highest-spending customers first. Virtual dispatch ensures
        // both the derived reference and a base-typed reference observe Descending.
        var param = new StoreCustomerParameter();
        var asBase = (AdventureWorks.Common.Filtering.Base.QueryStringParamsBase)param;

        using (new AssertionScope())
        {
            param.SortOrder.Should().Be(SortedResultConstants.Descending);
            asBase.SortOrder.Should().Be(SortedResultConstants.Descending);
        }
    }

    [Theory]
    [InlineData("asc", SortedResultConstants.Ascending)]
    [InlineData("ASC", SortedResultConstants.Ascending)]
    [InlineData(" ascending ", SortedResultConstants.Ascending)]
    [InlineData("ASCENDING", SortedResultConstants.Ascending)]
    [InlineData("desc", SortedResultConstants.Descending)]
    [InlineData("DESC", SortedResultConstants.Descending)]
    [InlineData(" descending ", SortedResultConstants.Descending)]
    [InlineData("DESCENDING", SortedResultConstants.Descending)]
    [InlineData("", SortedResultConstants.Descending)]
    [InlineData("garbage", SortedResultConstants.Descending)]
    [InlineData(null, SortedResultConstants.Descending)]
    public void SortOrder_field_is_calculated_correctly(string? sortInput, string sortExpectedOutput)
    {
        // Note: every "unknown / null / empty" branch resolves to Descending here,
        // unlike the base class which defaults the same inputs to Ascending.
        var param = new StoreCustomerParameter
        {
            SortOrder = sortInput
        };

        param.SortOrder.Should().Be(sortExpectedOutput);
    }

}
