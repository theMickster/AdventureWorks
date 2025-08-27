using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Defensive parser for the <c>Sales.Store.Demographics</c> XML payload.
/// The column conforms to the <c>StoreSurveySchemaCollection</c> XSD whose target namespace
/// is <c>http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey</c>.
/// Any element missing or unparseable simply leaves the corresponding model property null.
/// </summary>
public static class StoreDemographicsParser
{
    private static readonly XNamespace SurveyNamespace =
        "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey";

    /// <summary>
    /// Populates survey fields on <paramref name="model"/> by parsing
    /// <paramref name="rawXml"/>. <paramref name="model"/> is mutated in place.
    /// Null/whitespace input or malformed XML leaves the model untouched.
    /// </summary>
    public static void Populate(StoreDemographicsModel model, string? rawXml)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (string.IsNullOrWhiteSpace(rawXml))
        {
            return;
        }

        XDocument document;
        try
        {
            document = XDocument.Parse(rawXml);
        }
        catch (XmlException)
        {
            return;
        }

        var root = document.Root;
        if (root is null)
        {
            return;
        }

        string? Get(string n) => root.Element(SurveyNamespace + n)?.Value;

        model.AnnualSales = ParseDecimal(Get("AnnualSales"));
        model.AnnualRevenue = ParseDecimal(Get("AnnualRevenue"));
        model.BankName = NullIfBlank(Get("BankName"));
        model.BusinessType = NullIfBlank(Get("BusinessType"));
        model.YearOpened = ParseInt(Get("YearOpened"));
        model.Specialty = NullIfBlank(Get("Specialty"));
        model.SquareFeet = ParseInt(Get("SquareFeet"));
        model.Internet = NullIfBlank(Get("Internet"));
        model.NumberEmployees = ParseInt(Get("NumberEmployees"));
        model.Brands = NullIfBlank(Get("Brands"));
    }

    private static decimal? ParseDecimal(string? value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static int? ParseInt(string? value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
