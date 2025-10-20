using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;
using AdventureWorks.UnitTests.Setup;
using FluentAssertions;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

[ExcludeFromCodeCoverage]
public sealed class SalesOrderDetailRepositoryTests : PersistenceUnitTestBase
{
    private readonly SalesOrderRepository _sut;

    public SalesOrderDetailRepositoryTests()
    {
        _sut = new SalesOrderRepository(DbContext);
    }

    [Fact]
    public async Task GetSalesOrderDetailAsync_returns_order_with_line_items_when_found()
    {
        // Arrange
        var product = new Product { ProductId = 1, Name = "Mountain-100 Silver, 38" };
        var stateProvince = new StateProvinceEntity { StateProvinceId = 1, Name = "Washington" };
        var billTo = new AddressEntity { AddressId = 1, AddressLine1 = "123 Main St", City = "Seattle", PostalCode = "98101", StateProvince = stateProvince };
        var shipTo = new AddressEntity { AddressId = 2, AddressLine1 = "456 Oak Ave", City = "Redmond", PostalCode = "98052", StateProvince = stateProvince };
        var salesPerson = new SalesPersonEntity
        {
            BusinessEntityId = 275,
            Employee = new EmployeeEntity
            {
                BusinessEntityId = 275,
                PersonBusinessEntity = new PersonEntity { BusinessEntityId = 275, FirstName = "Linda", LastName = "Mitchell" }
            }
        };
        var territory = new SalesTerritoryEntity { TerritoryId = 1, Name = "Northwest", CountryRegionCode = "US", Group = "North America" };
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };

        var salesOrder = new SalesOrderHeader
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            DueDate = new DateTime(2011, 6, 12),
            Status = 5,
            TotalDue = 23153.2339m,
            CustomerId = 1,
            CustomerEntity = customer,
            BillToAddressEntity = billTo,
            ShipToAddressEntity = shipTo,
            SalesPerson = salesPerson,
            TerritoryEntity = territory,
            SalesOrderDetails = new List<SalesOrderDetail>
            {
                new()
                {
                    SalesOrderDetailId = 1,
                    OrderQty = 1,
                    UnitPrice = 2024.994m,
                    LineTotal = 2024.994m,
                    Product = product
                }
            }
        };

        DbContext.SalesOrderHeaders.Add(salesOrder);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetSalesOrderDetailAsync(43659, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.SalesOrderId.Should().Be(43659);
        result.SalesOrderDetails.Should().HaveCount(1);
        result.SalesOrderDetails.First().Product.Name.Should().Be("Mountain-100 Silver, 38");
        result.BillToAddressEntity.Should().NotBeNull();
        result.BillToAddressEntity!.StateProvince.Should().NotBeNull();
        result.BillToAddressEntity.StateProvince!.Name.Should().Be("Washington");
        result.ShipToAddressEntity.Should().NotBeNull();
        result.SalesPerson.Should().NotBeNull();
        result.SalesPerson!.Employee.PersonBusinessEntity.FirstName.Should().Be("Linda");
        result.TerritoryEntity.Should().NotBeNull();
        result.TerritoryEntity!.Name.Should().Be("Northwest");
    }

    [Fact]
    public async Task GetSalesOrderDetailAsync_returns_null_when_order_not_found()
    {
        // Act
        var result = await _sut.GetSalesOrderDetailAsync(999999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSalesOrderDetailAsync_uses_no_tracking()
    {
        // Arrange
        var customer = new CustomerEntity { CustomerId = 1, Person = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" } };
        var stateProvince = new StateProvinceEntity { StateProvinceId = 2, Name = "Oregon" };
        var salesOrder = new SalesOrderHeader
        {
            SalesOrderId = 43700,
            SalesOrderNumber = "SO43700",
            OrderDate = new DateTime(2012, 1, 1),
            DueDate = new DateTime(2012, 1, 15),
            Status = 5,
            TotalDue = 100m,
            CustomerId = 1,
            CustomerEntity = customer,
            BillToAddressEntity = new AddressEntity { AddressId = 10, AddressLine1 = "1 St", City = "Portland", PostalCode = "97201", StateProvince = stateProvince },
            ShipToAddressEntity = new AddressEntity { AddressId = 11, AddressLine1 = "2 St", City = "Portland", PostalCode = "97201", StateProvince = stateProvince },
            SalesOrderDetails = []
        };

        DbContext.SalesOrderHeaders.Add(salesOrder);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetSalesOrderDetailAsync(43700, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        DbContext.ChangeTracker.Entries().Should().BeEmpty();
    }
}
