using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadCustomerDetailQueryHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository = new();
    private readonly ReadCustomerDetailQueryHandler _sut;

    public ReadCustomerDetailQueryHandlerTests()
    {
        _sut = new ReadCustomerDetailQueryHandler(_mockCustomerRepository.Object);
    }

    [Fact]
    public async Task Handle_returns_customer_detail_for_a_store_customer_when_found()
    {
        var projection = new CustomerLtvProjection
        {
            CustomerId = 11003,
            StoreId = 900,
            DisplayName = "Topnotch Bikes",
            CustomerType = "Store",
            StoreName = "Topnotch Bikes",
            FirstName = null,
            LastName = null,
            TotalSpend = 500m,
            OrderCount = 2,
            LastOrderDate = new DateTime(2026, 4, 1),
            LtvRank = 2,
            IsInactive = false
        };

        _mockCustomerRepository
            .Setup(x => x.GetCustomerDetailAsync(11003, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projection, 6));

        var result = await _sut.Handle(new ReadCustomerDetailQuery { CustomerId = 11003 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.CustomerId.Should().Be(11003);
            result.DisplayName.Should().Be("Topnotch Bikes");
            result.LtvRank.Should().Be(2);
            result.TotalCustomerCount.Should().Be(6);
            result.CustomerType.Should().Be("Store");
            result.StoreId.Should().Be(900);
            result.StoreName.Should().Be("Topnotch Bikes");
            result.FirstName.Should().BeNull();
            result.LastName.Should().BeNull();
            result.AvgOrderValue.Should().Be(250m);
            result.IsInactive.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Handle_returns_customer_detail_for_an_individual_customer_when_found()
    {
        var projection = new CustomerLtvProjection
        {
            CustomerId = 11000,
            StoreId = null,
            DisplayName = "Jon Yang",
            CustomerType = "Individual",
            StoreName = null,
            FirstName = "Jon",
            LastName = "Yang",
            TotalSpend = 900m,
            OrderCount = 3,
            LastOrderDate = new DateTime(2026, 5, 1),
            LtvRank = 1,
            IsInactive = false
        };

        _mockCustomerRepository
            .Setup(x => x.GetCustomerDetailAsync(11000, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projection, 6));

        var result = await _sut.Handle(new ReadCustomerDetailQuery { CustomerId = 11000 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.CustomerType.Should().Be("Individual");
            result.StoreId.Should().BeNull();
            result.StoreName.Should().BeNull();
            result.FirstName.Should().Be("Jon");
            result.LastName.Should().Be("Yang");
            result.AvgOrderValue.Should().Be(300m);
        }
    }

    [Fact]
    public async Task Handle_returns_zero_avg_order_value_when_customer_has_no_orders()
    {
        var projection = new CustomerLtvProjection
        {
            CustomerId = 11001,
            DisplayName = "No Orders",
            CustomerType = "Individual",
            FirstName = "No",
            LastName = "Orders",
            TotalSpend = 0m,
            OrderCount = 0,
            LastOrderDate = null,
            LtvRank = 6,
            IsInactive = true
        };

        _mockCustomerRepository
            .Setup(x => x.GetCustomerDetailAsync(11001, It.IsAny<CancellationToken>()))
            .ReturnsAsync((projection, 6));

        var result = await _sut.Handle(new ReadCustomerDetailQuery { CustomerId = 11001 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.OrderCount.Should().Be(0);
            result.AvgOrderValue.Should().Be(0m);
            result.IsInactive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Handle_throws_key_not_found_exception_when_customer_does_not_exist()
    {
        _mockCustomerRepository
            .Setup(x => x.GetCustomerDetailAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((CustomerLtvProjection?)null, 0));

        var act = async () => await _sut.Handle(new ReadCustomerDetailQuery { CustomerId = 9999 }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_argument_null_exception_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
