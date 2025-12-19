using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Infrastructure.Persistence.Repositories.Production;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Production;

[ExcludeFromCodeCoverage]
public sealed class WorkOrderRepositoryTests : PersistenceUnitTestBase
{
    private readonly WorkOrderRepository _sut;

    public WorkOrderRepositoryTests()
    {
        _sut = new WorkOrderRepository(DbContext);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        typeof(WorkOrderRepository).Should().Implement<IWorkOrderRepository>();

        typeof(WorkOrderRepository)
            .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetWorkOrdersAsync_returns_paginated_results()
    {
        // Arrange
        var product = new Product { ProductId = 747, Name = "HL Mountain Frame - Black, 38" };

        var workOrders = new[]
        {
            new WorkOrder { WorkOrderId = 1, ProductId = 747, OrderQty = 4, StockedQty = 4, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 1), EndDate = new DateTime(2011, 6, 10), DueDate = new DateTime(2011, 6, 14), Product = product },
            new WorkOrder { WorkOrderId = 2, ProductId = 747, OrderQty = 8, StockedQty = 8, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 2), EndDate = new DateTime(2011, 6, 11), DueDate = new DateTime(2011, 6, 15), Product = product }
        };

        DbContext.WorkOrders.AddRange(workOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, totalCount) = await _sut.GetWorkOrdersAsync(parameters, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkOrdersAsync_respects_pagination()
    {
        // Arrange
        var product = new Product { ProductId = 747, Name = "HL Mountain Frame - Black, 38" };

        var workOrders = Enumerable.Range(1, 30)
            .Select(i => new WorkOrder
            {
                WorkOrderId = i,
                ProductId = 747,
                OrderQty = 4,
                StockedQty = 4,
                ScrappedQty = 0,
                StartDate = new DateTime(2011, 6, 1).AddDays(i),
                EndDate = new DateTime(2011, 6, 10).AddDays(i),
                DueDate = new DateTime(2011, 6, 14).AddDays(i),
                Product = product
            })
            .ToArray();

        DbContext.WorkOrders.AddRange(workOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 25 };

        // Act
        var (results, totalCount) = await _sut.GetWorkOrdersAsync(parameters, CancellationToken.None);

        // Assert
        results.Should().HaveCount(25);
        totalCount.Should().Be(30);
    }

    [Fact]
    public async Task GetWorkOrdersAsync_defaults_to_start_date_descending()
    {
        // Arrange
        var product = new Product { ProductId = 747, Name = "HL Mountain Frame - Black, 38" };

        var workOrders = new[]
        {
            new WorkOrder { WorkOrderId = 1, ProductId = 747, OrderQty = 4, StockedQty = 4, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 1), EndDate = new DateTime(2011, 6, 10), DueDate = new DateTime(2011, 6, 14), Product = product },
            new WorkOrder { WorkOrderId = 2, ProductId = 747, OrderQty = 8, StockedQty = 8, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 5), EndDate = new DateTime(2011, 6, 11), DueDate = new DateTime(2011, 6, 15), Product = product }
        };

        DbContext.WorkOrders.AddRange(workOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, _) = await _sut.GetWorkOrdersAsync(parameters, CancellationToken.None);

        // Assert
        results[0].WorkOrderId.Should().Be(2);
        results[1].WorkOrderId.Should().Be(1);
    }

    [Fact]
    public async Task SearchWorkOrdersAsync_filters_by_product_id()
    {
        // Arrange
        var productA = new Product { ProductId = 747, Name = "Frame A" };
        var productB = new Product { ProductId = 518, Name = "Frame B" };

        var workOrders = new[]
        {
            new WorkOrder { WorkOrderId = 1, ProductId = 747, OrderQty = 4, StockedQty = 4, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 1), EndDate = new DateTime(2011, 6, 10), DueDate = new DateTime(2011, 6, 14), Product = productA },
            new WorkOrder { WorkOrderId = 2, ProductId = 518, OrderQty = 98, StockedQty = 97, ScrappedQty = 1, StartDate = new DateTime(2011, 6, 3), EndDate = new DateTime(2011, 6, 19), DueDate = new DateTime(2011, 6, 14), ScrapReasonId = 7, Product = productB }
        };

        DbContext.WorkOrders.AddRange(workOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new WorkOrderSearchModel { ProductId = 518 };

        // Act
        var (results, totalCount) = await _sut.SearchWorkOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        totalCount.Should().Be(1);
        results[0].ProductId.Should().Be(518);
    }

    [Fact]
    public async Task SearchWorkOrdersAsync_filters_by_start_date_range()
    {
        // Arrange
        var product = new Product { ProductId = 747, Name = "Frame A" };

        var workOrders = new[]
        {
            new WorkOrder { WorkOrderId = 1, ProductId = 747, OrderQty = 4, StockedQty = 4, ScrappedQty = 0, StartDate = new DateTime(2011, 1, 1), EndDate = new DateTime(2011, 1, 10), DueDate = new DateTime(2011, 1, 14), Product = product },
            new WorkOrder { WorkOrderId = 2, ProductId = 747, OrderQty = 4, StockedQty = 4, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 1), EndDate = new DateTime(2011, 6, 10), DueDate = new DateTime(2011, 6, 14), Product = product }
        };

        DbContext.WorkOrders.AddRange(workOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new WorkOrderSearchModel { StartDate = new DateTime(2011, 5, 1), EndDate = new DateTime(2011, 12, 31) };

        // Act
        var (results, totalCount) = await _sut.SearchWorkOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        totalCount.Should().Be(1);
        results[0].WorkOrderId.Should().Be(2);
    }

    [Fact]
    public async Task SearchWorkOrdersAsync_filters_by_has_scrapped_true()
    {
        // Arrange
        var product = new Product { ProductId = 747, Name = "Frame A" };

        var workOrders = new[]
        {
            new WorkOrder { WorkOrderId = 1, ProductId = 747, OrderQty = 4, StockedQty = 4, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 1), EndDate = new DateTime(2011, 6, 10), DueDate = new DateTime(2011, 6, 14), Product = product },
            new WorkOrder { WorkOrderId = 2, ProductId = 747, OrderQty = 98, StockedQty = 97, ScrappedQty = 1, StartDate = new DateTime(2011, 6, 3), EndDate = new DateTime(2011, 6, 19), DueDate = new DateTime(2011, 6, 14), ScrapReasonId = 7, Product = product }
        };

        DbContext.WorkOrders.AddRange(workOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new WorkOrderSearchModel { HasScrapped = true };

        // Act
        var (results, totalCount) = await _sut.SearchWorkOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        totalCount.Should().Be(1);
        results[0].WorkOrderId.Should().Be(2);
    }

    [Fact]
    public async Task SearchWorkOrdersAsync_filters_by_scrap_reason_id()
    {
        // Arrange
        var product = new Product { ProductId = 747, Name = "Frame A" };

        var workOrders = new[]
        {
            new WorkOrder { WorkOrderId = 1, ProductId = 747, OrderQty = 98, StockedQty = 97, ScrappedQty = 1, StartDate = new DateTime(2011, 6, 3), EndDate = new DateTime(2011, 6, 19), DueDate = new DateTime(2011, 6, 14), ScrapReasonId = 7, Product = product },
            new WorkOrder { WorkOrderId = 2, ProductId = 747, OrderQty = 120, StockedQty = 117, ScrappedQty = 3, StartDate = new DateTime(2011, 6, 3), EndDate = new DateTime(2011, 6, 19), DueDate = new DateTime(2011, 6, 14), ScrapReasonId = 11, Product = product }
        };

        DbContext.WorkOrders.AddRange(workOrders);
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new WorkOrderSearchModel { ScrapReasonId = 11 };

        // Act
        var (results, totalCount) = await _sut.SearchWorkOrdersAsync(parameters, searchModel, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        totalCount.Should().Be(1);
        results[0].WorkOrderId.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkOrdersAsync_uses_no_tracking()
    {
        // Arrange
        var product = new Product { ProductId = 747, Name = "Frame A" };
        DbContext.WorkOrders.Add(new WorkOrder { WorkOrderId = 1, ProductId = 747, OrderQty = 4, StockedQty = 4, ScrappedQty = 0, StartDate = new DateTime(2011, 6, 1), EndDate = new DateTime(2011, 6, 10), DueDate = new DateTime(2011, 6, 14), Product = product });
        await DbContext.SaveChangesAsync();

        var parameters = new WorkOrderParameter { PageNumber = 1, PageSize = 10 };

        // Act
        var (results, _) = await _sut.GetWorkOrdersAsync(parameters, CancellationToken.None);

        // Assert
        var entry = DbContext.Entry(results[0]);
        entry.State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Detached);
    }
}
