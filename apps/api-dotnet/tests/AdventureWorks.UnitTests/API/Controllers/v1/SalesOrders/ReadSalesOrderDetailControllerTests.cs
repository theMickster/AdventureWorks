using AdventureWorks.API.Controllers.v1.SalesOrders;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.Sales;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesOrders;

public sealed class ReadSalesOrderDetailControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly Mock<ILogger<ReadSalesOrderController>> _mockLogger = new();
    private readonly ReadSalesOrderController _sut;

    public ReadSalesOrderDetailControllerTests()
    {
        _sut = new ReadSalesOrderController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task GetDetailAsync_returns_200_with_detail_model_when_order_found()
    {
        // Arrange
        var model = new SalesOrderDetailModel
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            DueDate = new DateTime(2011, 6, 12),
            Status = 5,
            StatusDescription = "Shipped",
            SubTotal = 20565.6206m,
            TaxAmt = 1971.5149m,
            Freight = 616.0984m,
            TotalDue = 23153.2339m,
            SalesPersonName = "Linda Mitchell",
            TerritoryName = "Northwest",
            BillToAddress = new SalesOrderAddressModel
            {
                AddressLine1 = "123 Main St",
                City = "Seattle",
                PostalCode = "98101",
                StateProvince = "Washington"
            },
            ShipToAddress = new SalesOrderAddressModel
            {
                AddressLine1 = "456 Oak Ave",
                City = "Redmond",
                PostalCode = "98052",
                StateProvince = "Washington"
            },
            LineItems =
            [
                new SalesOrderLineItemModel
                {
                    SalesOrderDetailId = 1,
                    ProductName = "Mountain-100 Silver, 38",
                    OrderQty = 1,
                    UnitPrice = 2024.994m,
                    LineTotal = 2024.994m
                }
            ]
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesOrderDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // Act
        var result = await _sut.GetDetailAsync(43659);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(model);
    }

    [Fact]
    public async Task GetDetailAsync_returns_404_when_order_not_found()
    {
        // Arrange
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesOrderDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesOrderDetailModel?)null);

        // Act
        var result = await _sut.GetDetailAsync(999999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetDetailAsync_sends_correct_sales_order_id_to_mediator()
    {
        // Arrange
        _mockMediator
            .Setup(x => x.Send(
                It.Is<ReadSalesOrderDetailQuery>(q => q.SalesOrderId == 43659),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SalesOrderDetailModel
            {
                SalesOrderId = 43659,
                SalesOrderNumber = "SO43659",
                OrderDate = new DateTime(2011, 5, 31),
                DueDate = new DateTime(2011, 6, 12),
                Status = 5,
                StatusDescription = "Shipped"
            });

        // Act
        var result = await _sut.GetDetailAsync(43659);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockMediator.Verify(
            x => x.Send(It.Is<ReadSalesOrderDetailQuery>(q => q.SalesOrderId == 43659), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDetailAsync_forwards_cancellation_token()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesOrderDetailQuery>(), token))
            .ReturnsAsync(new SalesOrderDetailModel
            {
                SalesOrderId = 1,
                SalesOrderNumber = "SO1",
                OrderDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Status = 5,
                StatusDescription = "Shipped"
            });

        // Act
        var result = await _sut.GetDetailAsync(1, token);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetDetailAsync_returns_400_when_sales_order_id_is_zero_or_negative()
    {
        // Act
        var result = await _sut.GetDetailAsync(0);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockMediator.Verify(
            x => x.Send(It.IsAny<ReadSalesOrderDetailQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
