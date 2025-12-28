using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Purchasing;
using AdventureWorks.Infrastructure.Persistence.Repositories.Purchasing;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Purchasing;

[ExcludeFromCodeCoverage]
public sealed class PurchaseOrderRepositoryTests : PersistenceUnitTestBase
{
    private readonly PurchaseOrderRepository _sut;

    public PurchaseOrderRepositoryTests()
    {
        _sut = new PurchaseOrderRepository(DbContext);
    }

    /// <summary>
    /// Seeds a purchase order header with a fully resolvable employee/person join.
    /// </summary>
    private void SeedPurchaseOrderWithEmployee(
        int purchaseOrderId,
        byte status,
        int employeeId = 261,
        string firstName = "Reinout",
        string lastName = "Hillmann")
    {
        DbContext.BusinessEntities.Add(new BusinessEntity
        {
            BusinessEntityId = employeeId,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.Persons.Add(new PersonEntity
        {
            BusinessEntityId = employeeId,
            FirstName = firstName,
            LastName = lastName,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.Employees.Add(new EmployeeEntity
        {
            BusinessEntityId = employeeId,
            NationalIdnumber = "111111111",
            LoginId = $"adventure-works\\employee{employeeId}",
            JobTitle = "Buyer",
            BirthDate = new DateTime(1980, 1, 1),
            MaritalStatus = "S",
            Gender = "M",
            HireDate = new DateTime(2010, 1, 1),
            SalariedFlag = true,
            CurrentFlag = true,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
        {
            PurchaseOrderId = purchaseOrderId,
            RevisionNumber = 1,
            Status = status,
            EmployeeId = employeeId,
            VendorId = 1650,
            ShipMethodId = 5,
            OrderDate = new DateTime(2011, 4, 16),
            SubTotal = 171.0765m,
            TaxAmt = 13.6861m,
            Freight = 4.2769m,
            TotalDue = 189.0395m,
            ModifiedDate = StandardModifiedDate
        });
    }

    private void SeedPurchaseOrderDetail(int purchaseOrderId, int purchaseOrderDetailId, DateTime dueDate)
    {
        DbContext.Add(new PurchaseOrderDetail
        {
            PurchaseOrderId = purchaseOrderId,
            PurchaseOrderDetailId = purchaseOrderDetailId,
            DueDate = dueDate,
            OrderQty = 3,
            ProductId = 4,
            UnitPrice = 57.0255m,
            LineTotal = 171.0765m,
            ReceivedQty = 2m,
            RejectedQty = 1m,
            StockedQty = 1m,
            ModifiedDate = StandardModifiedDate
        });
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_UnknownId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(9999, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_FullyResolvableEmployee_MapsAllFields()
    {
        // Arrange — real AdventureWorks shape (purchase order 4, employee 261 "Reinout Hillmann").
        SeedPurchaseOrderWithEmployee(4, status: 3);
        SeedPurchaseOrderDetail(4, purchaseOrderDetailId: 5, dueDate: new DateTime(2011, 4, 30));
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(4, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.PurchaseOrderId.Should().Be(4);
        result.Status.Should().Be(3);
        result.StatusLabel.Should().Be("Rejected");
        result.VendorId.Should().Be(1650);
        result.EmployeeId.Should().Be(261);
        result.ApprovingEmployeeFullName.Should().Be("Reinout Hillmann");
        result.ShipMethodId.Should().Be(5);
        result.OrderDate.Should().Be(new DateTime(2011, 4, 16));
        result.DueDate.Should().Be(new DateTime(2011, 4, 30));
        result.SubTotal.Should().Be(171.0765m);
        result.TaxAmt.Should().Be(13.6861m);
        result.Freight.Should().Be(4.2769m);
        result.TotalDue.Should().Be(189.0395m);
        result.LineItems.Should().ContainSingle();
        result.LineItems[0].PurchaseOrderDetailId.Should().Be(5);
        result.LineItems[0].ProductId.Should().Be(4);
    }

    [Theory]
    [InlineData((byte)1, "Pending")]
    [InlineData((byte)2, "Approved")]
    [InlineData((byte)3, "Rejected")]
    [InlineData((byte)4, "Complete")]
    public async Task GetPurchaseOrderDetailAsync_MapsAllStatusLabels(byte status, string expectedLabel)
    {
        // Arrange
        SeedPurchaseOrderWithEmployee(status, status: status, employeeId: 900 + status);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(status, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.StatusLabel.Should().Be(expectedLabel);
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_EmployeeIdDoesNotResolve_FallsBackToUnknownEmployee_AndStillReturnsResult()
    {
        // Arrange — regression test for the null-employee-join functional requirement: EmployeeId
        // references a non-existent employee. The purchase order must still be returned, not
        // treated as not-found.
        DbContext.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
        {
            PurchaseOrderId = 5001,
            RevisionNumber = 1,
            Status = 1,
            EmployeeId = 999999,
            VendorId = 1650,
            ShipMethodId = 5,
            OrderDate = new DateTime(2011, 4, 16),
            SubTotal = 100m,
            TaxAmt = 5m,
            Freight = 1m,
            TotalDue = 106m,
            ModifiedDate = StandardModifiedDate
        });
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(5001, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.ApprovingEmployeeFullName.Should().Be("Unknown employee");
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_EmployeeExistsButPersonJoinBroken_FallsBackToUnknownEmployee()
    {
        // Arrange — the employee row exists, but its BusinessEntityId does not resolve to a Person
        // row (orphaned employee->person join). This shape does not occur naturally in the real
        // AdventureWorks dataset (confirmed via direct query), so it is constructed here.
        DbContext.BusinessEntities.Add(new BusinessEntity
        {
            BusinessEntityId = 5002,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.Employees.Add(new EmployeeEntity
        {
            BusinessEntityId = 5002,
            NationalIdnumber = "222222222",
            LoginId = "adventure-works\\orphanedemployee",
            JobTitle = "Buyer",
            BirthDate = new DateTime(1980, 1, 1),
            MaritalStatus = "S",
            Gender = "M",
            HireDate = new DateTime(2010, 1, 1),
            SalariedFlag = true,
            CurrentFlag = true,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.PurchaseOrderHeaders.Add(new PurchaseOrderHeader
        {
            PurchaseOrderId = 5002,
            RevisionNumber = 1,
            Status = 1,
            EmployeeId = 5002,
            VendorId = 1650,
            ShipMethodId = 5,
            OrderDate = new DateTime(2011, 4, 16),
            SubTotal = 100m,
            TaxAmt = 5m,
            Freight = 1m,
            TotalDue = 106m,
            ModifiedDate = StandardModifiedDate
        });
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(5002, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.ApprovingEmployeeFullName.Should().Be("Unknown employee");
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_ZeroLineItems_ReturnsEmptyListAndDueDateFallsBackToOrderDate()
    {
        // Arrange
        SeedPurchaseOrderWithEmployee(6001, status: 1, employeeId: 6001);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(6001, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.LineItems.Should().BeEmpty();
        result.DueDate.Should().Be(result.OrderDate);
    }

    [Fact]
    public async Task GetPurchaseOrderDetailAsync_MultipleLineItems_DueDateIsMinimumOfLineItemDueDates()
    {
        // Arrange — regression test for the DueDate-as-MIN decision.
        SeedPurchaseOrderWithEmployee(7001, status: 1, employeeId: 7001);
        SeedPurchaseOrderDetail(7001, purchaseOrderDetailId: 1, dueDate: new DateTime(2011, 8, 20));
        SeedPurchaseOrderDetail(7001, purchaseOrderDetailId: 2, dueDate: new DateTime(2011, 8, 13));
        SeedPurchaseOrderDetail(7001, purchaseOrderDetailId: 3, dueDate: new DateTime(2011, 8, 27));
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetPurchaseOrderDetailAsync(7001, TestContext.Current.CancellationToken);

        // Assert — the earliest of the three due dates wins.
        result.Should().NotBeNull();
        result!.DueDate.Should().Be(new DateTime(2011, 8, 13));
        result.LineItems.Should().HaveCount(3);
    }
}
