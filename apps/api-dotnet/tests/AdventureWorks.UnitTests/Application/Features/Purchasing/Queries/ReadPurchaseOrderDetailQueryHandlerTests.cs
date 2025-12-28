using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Purchasing.Queries;

public sealed class ReadPurchaseOrderDetailQueryHandlerTests
{
    private readonly Mock<IPurchaseOrderRepository> _mockPurchaseOrderRepository = new();
    private readonly ReadPurchaseOrderDetailQueryHandler _sut;

    public ReadPurchaseOrderDetailQueryHandlerTests()
    {
        _sut = new ReadPurchaseOrderDetailQueryHandler(_mockPurchaseOrderRepository.Object);
    }

    [Fact]
    public async Task Handle_returns_purchase_order_detail_when_found()
    {
        // Arrange
        var detail = new PurchaseOrderDetailModel
        {
            PurchaseOrderId = 4,
            Status = 3,
            StatusLabel = "Rejected",
            OrderDate = new DateTime(2011, 4, 16),
            DueDate = new DateTime(2011, 4, 30),
            VendorId = 1650,
            EmployeeId = 261,
            ApprovingEmployeeFullName = "Reinout Hillmann",
            ShipMethodId = 5,
            SubTotal = 171.0765m,
            TaxAmt = 13.6861m,
            Freight = 4.2769m,
            TotalDue = 189.0395m,
            LineItems = new List<PurchaseOrderLineItemModel>
            {
                new()
                {
                    PurchaseOrderDetailId = 5,
                    ProductId = 4,
                    DueDate = new DateTime(2011, 4, 30),
                    OrderQty = 3,
                    UnitPrice = 57.0255m,
                    LineTotal = 171.0765m,
                    ReceivedQty = 2m,
                    RejectedQty = 1m,
                    StockedQty = 1m
                }
            }
        };

        _mockPurchaseOrderRepository
            .Setup(x => x.GetPurchaseOrderDetailAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act
        var result = await _sut.Handle(new ReadPurchaseOrderDetailQuery { PurchaseOrderId = 4 }, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(detail);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_exception_when_purchase_order_does_not_exist()
    {
        // Arrange
        _mockPurchaseOrderRepository
            .Setup(x => x.GetPurchaseOrderDetailAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrderDetailModel?)null);

        // Act
        var act = async () => await _sut.Handle(new ReadPurchaseOrderDetailQuery { PurchaseOrderId = 9999 }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_argument_null_exception_when_request_is_null()
    {
        // Act
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
