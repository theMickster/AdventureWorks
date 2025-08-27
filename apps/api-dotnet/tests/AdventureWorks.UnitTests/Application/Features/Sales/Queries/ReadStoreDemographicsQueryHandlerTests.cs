using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.UnitTests._TestHelpers;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreDemographicsQueryHandlerTests : UnitTestBase
{
    private const string SurveyXmlNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/StoreSurvey";

    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private ReadStoreDemographicsQueryHandler _sut;

    public ReadStoreDemographicsQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreDemographicsProjectionToStoreDemographicsModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreDemographicsQueryHandler(_mapper, _mockStoreRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreDemographicsQueryHandler(null!, _mockStoreRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreDemographicsQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_null_when_store_not_foundAsync()
    {
        _mockStoreRepository.Setup(x => x.GetDemographicsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreDemographicsProjection?)null);

        var result = await _sut.Handle(new ReadStoreDemographicsQuery { StoreId = 2534 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_model_with_only_name_when_demographics_is_nullAsync()
    {
        _mockStoreRepository.Setup(x => x.GetDemographicsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreDemographicsProjection
            {
                BusinessEntityId = 2534,
                Name = "Catalog Store",
                Demographics = null
            });

        var result = await _sut.Handle(new ReadStoreDemographicsQuery { StoreId = 2534 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StoreId.Should().Be(2534);
            result.StoreName.Should().Be("Catalog Store");
            StoreDemographicsAssertions.AssertSurveyFieldsNull(result);
        }
    }

    [Fact]
    public async Task Handle_parses_fully_populated_demographics_xmlAsync()
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

        _mockStoreRepository.Setup(x => x.GetDemographicsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreDemographicsProjection
            {
                BusinessEntityId = 2534,
                Name = "Bike World",
                Demographics = xml
            });

        var result = await _sut.Handle(new ReadStoreDemographicsQuery { StoreId = 2534 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StoreId.Should().Be(2534);
            result.StoreName.Should().Be("Bike World");
            result.AnnualSales.Should().Be(1500000m);
            result.AnnualRevenue.Should().Be(150000m);
            result.BankName.Should().Be("Primary International");
            result.BusinessType.Should().Be("OS");
            result.YearOpened.Should().Be(1974);
            result.Specialty.Should().Be("Road");
            result.SquareFeet.Should().Be(38000);
            result.Internet.Should().Be("DSL");
            result.NumberEmployees.Should().Be(40);
            result.Brands.Should().Be("3");
        }
    }

    [Fact]
    public async Task Handle_returns_model_with_null_survey_fields_when_xml_is_malformedAsync()
    {
        _mockStoreRepository.Setup(x => x.GetDemographicsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoreDemographicsProjection
            {
                BusinessEntityId = 2534,
                Name = "Broken XML Store",
                Demographics = "<StoreSurvey><AnnualSales>1500000"
            });

        var result = await _sut.Handle(new ReadStoreDemographicsQuery { StoreId = 2534 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StoreId.Should().Be(2534);
            result.StoreName.Should().Be("Broken XML Store");
            StoreDemographicsAssertions.AssertSurveyFieldsNull(result);
        }
    }

    [Fact]
    public async Task Handle_forwards_cancellation_token_to_repositoryAsync()
    {
        using var cts = new CancellationTokenSource();
        _mockStoreRepository.Setup(x => x.GetDemographicsAsync(2534, cts.Token))
            .ReturnsAsync((StoreDemographicsProjection?)null);

        await _sut.Handle(new ReadStoreDemographicsQuery { StoreId = 2534 }, cts.Token);

        _mockStoreRepository.Verify(x => x.GetDemographicsAsync(2534, cts.Token), Times.Once);
    }
}
