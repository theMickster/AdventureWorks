using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreCustomerListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfiguration;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private ReadStoreCustomerListQueryHandler _sut;

    public ReadStoreCustomerListQueryHandlerTests()
    {
        _mapperConfiguration = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreCustomerProjectionToStoreCustomerModelProfile).Assembly)
        );
        _mapper = _mapperConfiguration.CreateMapper();

        _sut = new ReadStoreCustomerListQueryHandler(_mapper, _mockStoreRepository.Object);
    }

    [Fact]
    public void AutoMapper_configuration_is_valid()
    {
        var act = () => _mapperConfiguration.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreCustomerListQueryHandler(null!, _mockStoreRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreCustomerListQueryHandler(_mapper, null!)))
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
    public async Task Handle_returns_null_when_store_does_not_existAsync()
    {
        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = new StoreCustomerParameter() },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeNull();
            _mockStoreRepository.Verify(
                x => x.GetCustomersByStoreIdAsync(It.IsAny<int>(), It.IsAny<StoreCustomerParameter>(), It.IsAny<CancellationToken>()),
                Times.Never,
                "because the existence check failed and the page should not be loaded");
        }
    }

    [Fact]
    public async Task Handle_returns_populated_result_when_store_has_customersAsync()
    {
        var parameters = new StoreCustomerParameter { PageNumber = 1, PageSize = 10 };
        var lastOrderDate = new DateTime(2025, 11, 15, 0, 0, 0, DateTimeKind.Utc);

        var projections = new List<StoreCustomerProjection>
        {
            new()
            {
                CustomerId = 11000,
                AccountNumber = "AW00011000",
                PersonName = "Jon Yang",
                LifetimeSpend = 8_249.00m,
                OrderCount = 3,
                LastOrderDate = lastOrderDate
            },
            new()
            {
                CustomerId = 11001,
                AccountNumber = "AW00011001",
                PersonName = "",
                LifetimeSpend = 0m,
                OrderCount = 0,
                LastOrderDate = null
            }
        };

        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository
            .Setup(x => x.GetCustomersByStoreIdAsync(2534, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projections.AsReadOnly(), 2));

        var result = await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = parameters },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(2);
            result.Results.Should().NotBeNull();
            result.Results!.Should().HaveCount(2);

            var first = result.Results![0];
            first.CustomerId.Should().Be(11000);
            first.AccountNumber.Should().Be("AW00011000");
            first.PersonName.Should().Be("Jon Yang");
            first.LifetimeSpend.Should().Be(8_249.00m);
            first.OrderCount.Should().Be(3);
            first.LastOrderDate.Should().Be(lastOrderDate);

            var second = result.Results![1];
            second.CustomerId.Should().Be(11001);
            second.PersonName.Should().Be("");
            second.LifetimeSpend.Should().Be(0m);
            second.OrderCount.Should().Be(0);
            second.LastOrderDate.Should().BeNull();
        }
    }

    [Fact]
    public async Task Handle_returns_empty_results_when_store_exists_but_has_no_customersAsync()
    {
        // AC scenario: store exists but has no customers — return 200 OK with an empty page,
        // never 404. 404 is reserved strictly for "store does not exist".
        var parameters = new StoreCustomerParameter { PageNumber = 1, PageSize = 10 };

        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository
            .Setup(x => x.GetCustomersByStoreIdAsync(2534, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<StoreCustomerProjection>().AsReadOnly(), 0));

        var result = await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = parameters },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.TotalRecords.Should().Be(0);
            result.Results.Should().NotBeNull();
            result.Results!.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_returns_empty_page_when_pageNumber_exceeds_available_dataAsync()
    {
        // AC scenario: pageNumber out of range — caller asks for page 99 of a 7-row table.
        // The handler returns 200 OK with TotalRecords reflecting the true total but Results empty.
        var parameters = new StoreCustomerParameter { PageNumber = 99, PageSize = 10 };

        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository
            .Setup(x => x.GetCustomersByStoreIdAsync(2534, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<StoreCustomerProjection>().AsReadOnly(), 7));

        var result = await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = parameters },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.PageNumber.Should().Be(99);
            result.TotalRecords.Should().Be(7);
            result.Results.Should().NotBeNull();
            result.Results!.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_forwards_default_descending_sortOrder_to_repositoryAsync()
    {
        // AC scenario: "list is sorted by lifetimeSpend descending by default".
        // A defaulted StoreCustomerParameter must reach the repository with
        // OrderBy=LifetimeSpend and SortOrder=Descending (the `new SortOrder` member-hide).
        var defaultParameters = new StoreCustomerParameter();

        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository
            .Setup(x => x.GetCustomersByStoreIdAsync(2534, It.IsAny<StoreCustomerParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<StoreCustomerProjection>().AsReadOnly(), 0));

        await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = defaultParameters },
            CancellationToken.None);

        _mockStoreRepository.Verify(
            x => x.GetCustomersByStoreIdAsync(
                2534,
                It.Is<StoreCustomerParameter>(p =>
                    p.OrderBy == StoreCustomerParameter.LifetimeSpend &&
                    p.SortOrder == SortedResultConstants.Descending),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_forwards_explicit_ascending_sortOrder_to_repositoryAsync()
    {
        // The model-binder writes to the derived `SortOrder` init-only property; verify
        // that an explicit `?sortOrder=asc` survives the trip through the handler unchanged.
        var parameters = new StoreCustomerParameter { OrderBy = "personname", SortOrder = "asc" };

        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository
            .Setup(x => x.GetCustomersByStoreIdAsync(2534, It.IsAny<StoreCustomerParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<StoreCustomerProjection>().AsReadOnly(), 0));

        await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = parameters },
            CancellationToken.None);

        _mockStoreRepository.Verify(
            x => x.GetCustomersByStoreIdAsync(
                2534,
                It.Is<StoreCustomerParameter>(p =>
                    p.OrderBy == StoreCustomerParameter.PersonName &&
                    p.SortOrder == SortedResultConstants.Ascending),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_maps_empty_PersonName_projection_to_empty_string_modelAsync()
    {
        // Repository projection emits PersonName = "" (not null) when Customer.Person is null.
        // Verify AutoMapper preserves the empty string verbatim - never null, never whitespace.
        var parameters = new StoreCustomerParameter { PageNumber = 1, PageSize = 10 };

        var projections = new List<StoreCustomerProjection>
        {
            new()
            {
                CustomerId = 30000,
                AccountNumber = "AW00030000",
                PersonName = string.Empty,
                LifetimeSpend = 1_234.56m,
                OrderCount = 1,
                LastOrderDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockStoreRepository
            .Setup(x => x.GetCustomersByStoreIdAsync(2534, parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projections.AsReadOnly(), 1));

        var result = await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = parameters },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Results.Should().NotBeNull();
            result.Results!.Should().HaveCount(1);

            var mapped = result.Results![0];
            mapped.PersonName.Should().NotBeNull("because the model contract specifies an empty string when no person is linked");
            mapped.PersonName.Should().BeEmpty();
            mapped.PersonName.Should().Be(string.Empty);
        }
    }

    [Fact]
    public async Task Handle_forwards_cancellation_token_to_repositoryAsync()
    {
        using var cts = new CancellationTokenSource();
        var parameters = new StoreCustomerParameter();

        _mockStoreRepository
            .Setup(x => x.ExistsAsync(2534, cts.Token))
            .ReturnsAsync(true);
        _mockStoreRepository
            .Setup(x => x.GetCustomersByStoreIdAsync(2534, parameters, cts.Token))
            .ReturnsAsync((new List<StoreCustomerProjection>().AsReadOnly(), 0));

        await _sut.Handle(
            new ReadStoreCustomerListQuery { StoreId = 2534, Parameters = parameters },
            cts.Token);

        using (new AssertionScope())
        {
            _mockStoreRepository.Verify(
                x => x.ExistsAsync(2534, cts.Token),
                Times.Once);
            _mockStoreRepository.Verify(
                x => x.GetCustomersByStoreIdAsync(2534, parameters, cts.Token),
                Times.Once);
        }
    }
}
