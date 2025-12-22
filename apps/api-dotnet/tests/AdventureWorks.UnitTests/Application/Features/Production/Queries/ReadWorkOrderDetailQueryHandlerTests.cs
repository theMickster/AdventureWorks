using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadWorkOrderDetailQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IWorkOrderRepository> _mockWorkOrderRepository = new();
    private readonly ReadWorkOrderDetailQueryHandler _sut;

    public ReadWorkOrderDetailQueryHandlerTests()
    {
        _mapper = SharedMapper;
        _sut = new ReadWorkOrderDetailQueryHandler(_mapper, _mockWorkOrderRepository.Object);
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_mapped_detail_for_completed_late_work_order_with_scrap_reason()
    {
        // Arrange - real fixture: WorkOrderID 41, ProductID 518 (ML Road Seat Assembly), ScrapReasonID 7 (Handling damage)
        var workOrderEntity = new WorkOrder
        {
            WorkOrderId = 41,
            ProductId = 518,
            OrderQty = 98,
            StockedQty = 97,
            ScrappedQty = 1,
            StartDate = new DateTime(2011, 6, 3),
            EndDate = new DateTime(2011, 6, 19),
            DueDate = new DateTime(2011, 6, 14),
            ScrapReasonId = 7,
            Product = new Product { ProductId = 518, Name = "ML Road Seat Assembly" },
            ScrapReason = new ScrapReason { ScrapReasonId = 7, Name = "Handling damage" }
        };

        _mockWorkOrderRepository.Setup(x => x.GetByIdAsync(41, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrderEntity);

        // Act
        var result = await _sut.Handle(new ReadWorkOrderDetailQuery { WorkOrderId = 41 }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WorkOrderId.Should().Be(41);
        result.ProductId.Should().Be(518);
        result.ProductName.Should().Be("ML Road Seat Assembly");
        result.OrderedQty.Should().Be(98);
        result.StockedQty.Should().Be(97);
        result.ScrappedQty.Should().Be(1);
        result.IsCompletedLate.Should().BeTrue();
        result.DaysLate.Should().Be(5);
        result.ScrapReasonId.Should().Be(7);
        result.ScrapReasonName.Should().Be("Handling damage");
    }

    [Fact]
    public async Task Handle_returns_null_days_late_when_completed_on_time()
    {
        // Arrange - real fixture: WorkOrderID 1, ProductID 722 (LL Road Frame - Black, 58), on-time
        var workOrderEntity = new WorkOrder
        {
            WorkOrderId = 1,
            ProductId = 722,
            OrderQty = 8,
            StockedQty = 8,
            ScrappedQty = 0,
            StartDate = new DateTime(2011, 6, 3),
            EndDate = new DateTime(2011, 6, 13),
            DueDate = new DateTime(2011, 6, 14),
            Product = new Product { ProductId = 722, Name = "LL Road Frame - Black, 58" }
        };

        _mockWorkOrderRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrderEntity);

        // Act
        var result = await _sut.Handle(new ReadWorkOrderDetailQuery { WorkOrderId = 1 }, CancellationToken.None);

        // Assert
        result.IsCompletedLate.Should().BeFalse();
        result.DaysLate.Should().BeNull();
        result.ScrapReasonId.Should().BeNull();
        result.ScrapReasonName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_null_scrap_reason_name_when_no_scrap_reason()
    {
        // Arrange - real fixture: WorkOrderID 13, ProductID 747 (HL Mountain Frame - Black, 38), completed late, no scrap
        var workOrderEntity = new WorkOrder
        {
            WorkOrderId = 13,
            ProductId = 747,
            OrderQty = 4,
            StockedQty = 4,
            ScrappedQty = 0,
            StartDate = new DateTime(2011, 6, 3),
            EndDate = new DateTime(2011, 6, 19),
            DueDate = new DateTime(2011, 6, 14),
            Product = new Product { ProductId = 747, Name = "HL Mountain Frame - Black, 38" }
        };

        _mockWorkOrderRepository.Setup(x => x.GetByIdAsync(13, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrderEntity);

        // Act
        var result = await _sut.Handle(new ReadWorkOrderDetailQuery { WorkOrderId = 13 }, CancellationToken.None);

        // Assert
        result.IsCompletedLate.Should().BeTrue();
        result.DaysLate.Should().Be(5);
        result.ScrapReasonId.Should().BeNull();
        result.ScrapReasonName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_work_order_does_not_exist()
    {
        // Arrange - confirmed absent via database query
        _mockWorkOrderRepository.Setup(x => x.GetByIdAsync(9999999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var act = async () => await _sut.Handle(new ReadWorkOrderDetailQuery { WorkOrderId = 9999999 }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Work order with ID 9999999 not found.");
    }
}
