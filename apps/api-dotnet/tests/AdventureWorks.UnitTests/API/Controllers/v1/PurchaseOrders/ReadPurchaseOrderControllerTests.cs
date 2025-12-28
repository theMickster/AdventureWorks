using AdventureWorks.API.Controllers.v1.PurchaseOrders;
using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AdventureWorks.UnitTests.API.Controllers.v1.PurchaseOrders;

public sealed class ReadPurchaseOrderControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadPurchaseOrderController _sut;

    public ReadPurchaseOrderControllerTests()
    {
        _sut = new ReadPurchaseOrderController(_mockMediator.Object);
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_returns_ok_result_with_detail()
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
            TotalDue = 189.0395m
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadPurchaseOrderDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(4, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(detail);
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_invalid_id_returns_bad_request()
    {
        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(0, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockMediator.Verify(x => x.Send(It.IsAny<ReadPurchaseOrderDetailQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_negative_id_returns_bad_request()
    {
        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(-1, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockMediator.Verify(x => x.Send(It.IsAny<ReadPurchaseOrderDetailQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
