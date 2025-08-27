using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.UnitTests._TestHelpers;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class StoreDemographicsParserTests : UnitTestBase
{
    private const string SurveyXmlNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey";

    [Fact]
    public void Populate_throws_when_model_is_null()
    {
        var act = () => StoreDemographicsParser.Populate(null!, "<StoreSurvey/>");

        act.Should().Throw<ArgumentNullException>().WithParameterName("model");
    }

    [Fact]
    public void Populate_leaves_model_untouched_when_xml_is_null()
    {
        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, null);

        AssertSurveyFieldsNull(model);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t  \r\n")]
    public void Populate_leaves_model_untouched_when_xml_is_whitespace(string rawXml)
    {
        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, rawXml);

        AssertSurveyFieldsNull(model);
    }

    [Theory]
    [InlineData("<StoreSurvey><AnnualSales>100")]
    [InlineData("not even xml")]
    [InlineData("<StoreSurvey><BankName>Foo</WrongClose></StoreSurvey>")]
    public void Populate_leaves_model_untouched_when_xml_is_malformed(string rawXml)
    {
        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, rawXml);

        AssertSurveyFieldsNull(model);
    }

    [Fact]
    public void Populate_parses_all_fields_from_fully_populated_xml()
    {
        var xml = $"""
            <StoreSurvey xmlns="{SurveyXmlNamespace}">
                <AnnualSales>1500000</AnnualSales>
                <AnnualRevenue>150000</AnnualRevenue>
                <BankName>Primary International</BankName>
                <BusinessType>OS</BusinessType>
                <YearOpened>1974</YearOpened>
                <Specialty>Road</Specialty>
                <SquareFeet>38000</SquareFeet>
                <Brands>3</Brands>
                <Internet>DSL</Internet>
                <NumberEmployees>40</NumberEmployees>
            </StoreSurvey>
            """;

        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, xml);

        using (new AssertionScope())
        {
            model.AnnualSales.Should().Be(1500000m);
            model.AnnualRevenue.Should().Be(150000m);
            model.BankName.Should().Be("Primary International");
            model.BusinessType.Should().Be("OS");
            model.YearOpened.Should().Be(1974);
            model.Specialty.Should().Be("Road");
            model.SquareFeet.Should().Be(38000);
            model.Brands.Should().Be("3");
            model.Internet.Should().Be("DSL");
            model.NumberEmployees.Should().Be(40);
        }
    }

    [Theory]
    [InlineData("DSL")]
    [InlineData("T1")]
    [InlineData("56kb")]
    [InlineData("ISDN")]
    public void Populate_passes_through_internet_connectivity_enum_values(string internetValue)
    {
        var xml = $"""
            <StoreSurvey xmlns="{SurveyXmlNamespace}">
                <Internet>{internetValue}</Internet>
            </StoreSurvey>
            """;

        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, xml);

        model.Internet.Should().Be(internetValue);
    }

    [Fact]
    public void Populate_returns_null_internet_when_element_is_missing()
    {
        var xml = $"""
            <StoreSurvey xmlns="{SurveyXmlNamespace}">
                <BankName>Acme Bank</BankName>
            </StoreSurvey>
            """;

        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, xml);

        model.Internet.Should().BeNull();
    }

    [Fact]
    public void Populate_handles_partial_xml_leaving_missing_fields_null()
    {
        var xml = $"""
            <StoreSurvey xmlns="{SurveyXmlNamespace}">
                <AnnualSales>250000</AnnualSales>
                <BankName>Acme Bank</BankName>
            </StoreSurvey>
            """;

        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, xml);

        using (new AssertionScope())
        {
            model.AnnualSales.Should().Be(250000m);
            model.BankName.Should().Be("Acme Bank");
            model.AnnualRevenue.Should().BeNull();
            model.BusinessType.Should().BeNull();
            model.YearOpened.Should().BeNull();
            model.Specialty.Should().BeNull();
            model.SquareFeet.Should().BeNull();
            model.Internet.Should().BeNull();
            model.NumberEmployees.Should().BeNull();
            model.Brands.Should().BeNull();
        }
    }

    [Fact]
    public void Populate_treats_unparseable_numeric_values_as_null()
    {
        var xml = $"""
            <StoreSurvey xmlns="{SurveyXmlNamespace}">
                <AnnualSales>not-a-number</AnnualSales>
                <YearOpened>nineteen-seventy-four</YearOpened>
            </StoreSurvey>
            """;

        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, xml);

        using (new AssertionScope())
        {
            model.AnnualSales.Should().BeNull();
            model.YearOpened.Should().BeNull();
        }
    }

    [Fact]
    public void Populate_does_not_pick_up_elements_in_wrong_namespace()
    {
        // Demographics XML missing the expected namespace must NOT be parsed —
        // a silent namespace mismatch is the most common bug for XSD-validated payloads.
        const string xml = """
            <StoreSurvey>
                <AnnualSales>1500000</AnnualSales>
                <BankName>Wrong Namespace Bank</BankName>
            </StoreSurvey>
            """;

        var model = NewBaselineModel();

        StoreDemographicsParser.Populate(model, xml);

        AssertSurveyFieldsNull(model);
    }

    private static StoreDemographicsModel NewBaselineModel() => new()
    {
        StoreId = 2534,
        StoreName = "Bike World"
    };

    private static void AssertSurveyFieldsNull(StoreDemographicsModel model)
    {
        using (new AssertionScope())
        {
            model.StoreId.Should().Be(2534);
            model.StoreName.Should().Be("Bike World");
            StoreDemographicsAssertions.AssertSurveyFieldsNull(model);
        }
    }
}
