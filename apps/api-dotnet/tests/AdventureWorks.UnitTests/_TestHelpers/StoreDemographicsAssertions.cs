using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests._TestHelpers;

[ExcludeFromCodeCoverage]
public static class StoreDemographicsAssertions
{
    /// <summary>
    /// Asserts every survey field on <paramref name="model"/> is null. Identity fields
    /// (StoreId / StoreName) are not checked here — callers verify those separately.
    /// </summary>
    public static void AssertSurveyFieldsNull(StoreDemographicsModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using (new AssertionScope())
        {
            model.AnnualSales.Should().BeNull();
            model.AnnualRevenue.Should().BeNull();
            model.BankName.Should().BeNull();
            model.BusinessType.Should().BeNull();
            model.YearOpened.Should().BeNull();
            model.Specialty.Should().BeNull();
            model.SquareFeet.Should().BeNull();
            model.Internet.Should().BeNull();
            model.NumberEmployees.Should().BeNull();
            model.Brands.Should().BeNull();
        }
    }
}
