using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.UnitTests.Setup.Fakes;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadWorkOrderListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IWorkOrderRepository> _mockWorkOrderRepository = new();
    private readonly Mock<IValidator<ReadWorkOrderListQuery>> _mockValidator = new();
    private readonly ReadWorkOrderListQueryHandler _sut;

    public ReadWorkOrderListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(WorkOrderEntityToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ReadWorkOrderListQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _sut = new ReadWorkOrderListQueryHandler(_mapper, _mockWorkOrderRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_returns_success_with_results()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters };

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

        _mockWorkOrderRepository.Setup(x =>
            x.GetWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { workOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalRecords.Should().Be(1);
        result.Results.Should().HaveCount(1);
        result.Results![0].WorkOrderId.Should().Be(13);
        result.Results[0].ProductName.Should().Be("HL Mountain Frame - Black, 38");
        result.Results[0].OrderedQty.Should().Be(4);
        result.Results[0].YieldRate.Should().Be(100m);
        result.Results[0].IsCompletedLate.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_returns_success_with_empty_results()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters };

        _mockWorkOrderRepository.Setup(x =>
            x.GetWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<WorkOrder>().AsReadOnly(), 0));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_calls_search_when_search_model_provided()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new WorkOrderSearchModel { ProductId = 747 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters, SearchModel = searchModel };

        var workOrderEntity = new WorkOrder
        {
            WorkOrderId = 13,
            ProductId = 747,
            OrderQty = 4,
            StockedQty = 4,
            ScrappedQty = 0,
            StartDate = new DateTime(2011, 6, 3),
            EndDate = new DateTime(2011, 6, 10),
            DueDate = new DateTime(2011, 6, 14),
            Product = new Product { ProductId = 747, Name = "HL Mountain Frame - Black, 38" }
        };

        _mockWorkOrderRepository.Setup(x =>
            x.SearchWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<WorkOrderSearchModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { workOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results.Should().HaveCount(1);
        _mockWorkOrderRepository.Verify(x =>
            x.SearchWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), searchModel, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_false_is_completed_late_when_on_time()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters };

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
            Product = new Product { ProductId = 722, Name = "Some Product" }
        };

        _mockWorkOrderRepository.Setup(x =>
            x.GetWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { workOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results![0].IsCompletedLate.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_returns_false_is_completed_late_when_end_date_null()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters };

        var workOrderEntity = new WorkOrder
        {
            WorkOrderId = 1,
            ProductId = 722,
            OrderQty = 8,
            StockedQty = 4,
            ScrappedQty = 0,
            StartDate = new DateTime(2011, 6, 3),
            EndDate = null,
            DueDate = new DateTime(2011, 6, 14),
            Product = new Product { ProductId = 722, Name = "Some Product" }
        };

        _mockWorkOrderRepository.Setup(x =>
            x.GetWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { workOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results![0].IsCompletedLate.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_returns_zero_yield_rate_when_order_qty_is_zero()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters };

        var workOrderEntity = new WorkOrder
        {
            WorkOrderId = 1,
            ProductId = 722,
            OrderQty = 0,
            StockedQty = 0,
            ScrappedQty = 0,
            StartDate = new DateTime(2011, 6, 3),
            EndDate = new DateTime(2011, 6, 10),
            DueDate = new DateTime(2011, 6, 14),
            Product = new Product { ProductId = 722, Name = "Some Product" }
        };

        _mockWorkOrderRepository.Setup(x =>
            x.GetWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { workOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results![0].YieldRate.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_returns_empty_product_name_when_product_not_loaded()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters };

        var workOrderEntity = new WorkOrder
        {
            WorkOrderId = 1,
            ProductId = 722,
            OrderQty = 8,
            StockedQty = 8,
            ScrappedQty = 0,
            StartDate = new DateTime(2011, 6, 3),
            EndDate = new DateTime(2011, 6, 10),
            DueDate = new DateTime(2011, 6, 14),
            Product = null!
        };

        _mockWorkOrderRepository.Setup(x =>
            x.GetWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { workOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results![0].ProductName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Handle_preserves_total_records_when_page_is_out_of_range()
    {
        // Arrange
        var parameters = new WorkOrderParameter { PageNumber = 2, PageSize = 10 };
        var query = new ReadWorkOrderListQuery { Parameters = parameters };

        _mockWorkOrderRepository.Setup(x =>
            x.GetWorkOrdersAsync(It.IsAny<WorkOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<WorkOrder>().AsReadOnly(), 5));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.TotalRecords.Should().Be(5);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_query_is_invalid()
    {
        // Arrange
        var handler = new ReadWorkOrderListQueryHandler(
            _mapper,
            _mockWorkOrderRepository.Object,
            new FakeFailureValidator<ReadWorkOrderListQuery>("Parameters", "Parameters cannot be null"));

        var query = new ReadWorkOrderListQuery { Parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 } };

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
