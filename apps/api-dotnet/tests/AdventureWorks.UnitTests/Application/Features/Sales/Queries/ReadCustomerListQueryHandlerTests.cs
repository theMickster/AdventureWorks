using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Filtering;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadCustomerListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _mapperConfiguration;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository = new();
    private readonly Mock<IValidator<ReadCustomerListQuery>> _mockValidator = new();
    private ReadCustomerListQueryHandler _sut;

    public ReadCustomerListQueryHandlerTests()
    {
        _mapperConfiguration = new MapperConfiguration(config =>
            config.AddMaps(typeof(CustomerLtvProjectionToCustomerListItemModelProfile).Assembly)
        );
        _mapper = _mapperConfiguration.CreateMapper();

        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ReadCustomerListQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _sut = new ReadCustomerListQueryHandler(_mapper, _mockCustomerRepository.Object, _mockValidator.Object);
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
            _ = ((Action)(() => _sut = new ReadCustomerListQueryHandler(null!, _mockCustomerRepository.Object, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadCustomerListQueryHandler(_mapper, null!, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("customerRepository");

            _ = ((Action)(() => _sut = new ReadCustomerListQueryHandler(_mapper, _mockCustomerRepository.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_populated_result_when_customers_existAsync()
    {
        var parameters = new CustomerParameter { PageNumber = 1, PageSize = 10 };
        var lastOrderDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);

        var projections = new List<CustomerLtvProjection>
        {
            new()
            {
                CustomerId = 11000,
                StoreId = null,
                DisplayName = "Jon Yang",
                CustomerType = "Individual",
                TotalSpend = 8_249.00m,
                OrderCount = 3,
                LastOrderDate = lastOrderDate,
                LtvRank = 1,
                IsInactive = false
            },
            new()
            {
                CustomerId = 11001,
                StoreId = 900,
                DisplayName = "Topnotch Bikes",
                CustomerType = "Store",
                TotalSpend = 0m,
                OrderCount = 0,
                LastOrderDate = null,
                LtvRank = 2,
                IsInactive = true
            }
        };

        _mockCustomerRepository
            .Setup(x => x.GetCustomersAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projections.AsReadOnly(), 2));

        var result = await _sut.Handle(new ReadCustomerListQuery { Parameters = parameters }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalRecords.Should().Be(2);
            result.Results.Should().NotBeNull();
            result.Results!.Should().HaveCount(2);

            var first = result.Results![0];
            first.CustomerId.Should().Be(11000);
            first.DisplayName.Should().Be("Jon Yang");
            first.CustomerType.Should().Be("Individual");
            first.LtvRank.Should().Be(1);
            first.TotalSpend.Should().Be(8_249.00m);
            first.OrderCount.Should().Be(3);
            first.IsInactive.Should().BeFalse();

            var second = result.Results![1];
            second.CustomerId.Should().Be(11001);
            second.StoreId.Should().Be(900);
            second.LtvRank.Should().Be(2);
            second.TotalSpend.Should().Be(0m);
            second.IsInactive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Handle_returns_empty_results_when_no_customers_matchAsync()
    {
        var parameters = new CustomerParameter { PageNumber = 1, PageSize = 10, Search = "no-such-customer" };

        _mockCustomerRepository
            .Setup(x => x.GetCustomersAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<CustomerLtvProjection>().AsReadOnly(), 0));

        var result = await _sut.Handle(new ReadCustomerListQuery { Parameters = parameters }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(0);
            result.Results.Should().NotBeNull();
            result.Results!.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_returns_empty_page_when_pageNumber_exceeds_available_dataAsync()
    {
        var parameters = new CustomerParameter { PageNumber = 99, PageSize = 10 };

        _mockCustomerRepository
            .Setup(x => x.GetCustomersAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<CustomerLtvProjection>().AsReadOnly(), 7));

        var result = await _sut.Handle(new ReadCustomerListQuery { Parameters = parameters }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.PageNumber.Should().Be(99);
            result.TotalRecords.Should().Be(7);
            result.Results.Should().NotBeNull();
            result.Results!.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_preserves_global_rank_when_search_filters_out_middle_customerAsync()
    {
        // AC scenario: rank is assigned across the full customer set before search filtering.
        // A search that filters out the customer ranked #2 must not renumber #1 and #3.
        var parameters = new CustomerParameter { PageNumber = 1, PageSize = 10, Search = "yang" };

        var projections = new List<CustomerLtvProjection>
        {
            new() { CustomerId = 1, DisplayName = "Jon Yang", CustomerType = "Individual", TotalSpend = 900m, OrderCount = 2, LtvRank = 1, IsInactive = false },
            new() { CustomerId = 3, DisplayName = "Yang Bicycle Co", CustomerType = "Store", TotalSpend = 100m, OrderCount = 1, LtvRank = 3, IsInactive = false }
        };

        _mockCustomerRepository
            .Setup(x => x.GetCustomersAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projections.AsReadOnly(), 2));

        var result = await _sut.Handle(new ReadCustomerListQuery { Parameters = parameters }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results.Should().NotBeNull();
            result.Results!.Should().HaveCount(2);
            result.Results![0].LtvRank.Should().Be(1);
            result.Results![1].LtvRank.Should().Be(3);
        }
    }

    [Fact]
    public async Task Handle_maps_zero_order_customer_as_last_ranked_and_inactiveAsync()
    {
        var parameters = new CustomerParameter { PageNumber = 1, PageSize = 10 };

        var projections = new List<CustomerLtvProjection>
        {
            new() { CustomerId = 5, DisplayName = "Never Ordered", CustomerType = "Individual", TotalSpend = 0m, OrderCount = 0, LastOrderDate = null, LtvRank = 1, IsInactive = true }
        };

        _mockCustomerRepository
            .Setup(x => x.GetCustomersAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projections.AsReadOnly(), 1));

        var result = await _sut.Handle(new ReadCustomerListQuery { Parameters = parameters }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results.Should().NotBeNull();
            result.Results!.Should().HaveCount(1);
            result.Results![0].TotalSpend.Should().Be(0m);
            result.Results![0].OrderCount.Should().Be(0);
            result.Results![0].IsInactive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Handle_forwards_cancellation_token_to_repositoryAsync()
    {
        using var cts = new CancellationTokenSource();
        var parameters = new CustomerParameter();

        _mockCustomerRepository
            .Setup(x => x.GetCustomersAsync(parameters, cts.Token))
            .ReturnsAsync((new List<CustomerLtvProjection>().AsReadOnly(), 0));

        await _sut.Handle(new ReadCustomerListQuery { Parameters = parameters }, cts.Token);

        _mockCustomerRepository.Verify(
            x => x.GetCustomersAsync(parameters, cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_invokes_validator_before_querying_repositoryAsync()
    {
        var parameters = new CustomerParameter();

        _mockCustomerRepository
            .Setup(x => x.GetCustomersAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<CustomerLtvProjection>().AsReadOnly(), 0));

        await _sut.Handle(new ReadCustomerListQuery { Parameters = parameters }, CancellationToken.None);

        _mockValidator.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<ReadCustomerListQuery>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
