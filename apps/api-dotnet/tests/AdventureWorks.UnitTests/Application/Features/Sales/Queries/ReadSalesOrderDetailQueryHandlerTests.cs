using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentAssertions;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesOrderDetailQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesOrderRepository> _mockSalesOrderRepository = new();
    private readonly ReadSalesOrderDetailQueryHandler _sut;

    public ReadSalesOrderDetailQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(SalesOrderDetailEntityToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadSalesOrderDetailQueryHandler(_mapper, _mockSalesOrderRepository.Object);
    }

    [Fact]
    public async Task Handle_returns_detail_model_when_order_exists()
    {
        // Arrange
        var stateProvince = new StateProvinceEntity { StateProvinceId = 10, Name = "Washington" };
        var billTo = new AddressEntity
        {
            AddressId = 1,
            AddressLine1 = "123 Main St",
            City = "Seattle",
            PostalCode = "98101",
            StateProvince = stateProvince
        };
        var shipTo = new AddressEntity
        {
            AddressId = 2,
            AddressLine1 = "456 Oak Ave",
            City = "Redmond",
            PostalCode = "98052",
            StateProvince = stateProvince
        };
        var salesPersonEmployee = new EmployeeEntity
        {
            BusinessEntityId = 275,
            PersonBusinessEntity = new PersonEntity { FirstName = "Linda", LastName = "Mitchell" }
        };
        var territory = new SalesTerritoryEntity { TerritoryId = 1, Name = "Northwest" };
        var product = new Product { ProductId = 776, Name = "Mountain-100 Silver, 38" };

        var entity = new SalesOrderHeader
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            DueDate = new DateTime(2011, 6, 12),
            Status = 5,
            SubTotal = 20565.6206m,
            TaxAmt = 1971.5149m,
            Freight = 616.0984m,
            TotalDue = 23153.2339m,
            BillToAddressEntity = billTo,
            ShipToAddressEntity = shipTo,
            SalesPerson = new SalesPersonEntity { Employee = salesPersonEmployee },
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

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderDetailAsync(43659, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.Handle(new ReadSalesOrderDetailQuery { SalesOrderId = 43659 }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.SalesOrderId.Should().Be(43659);
        result.SalesOrderNumber.Should().Be("SO43659");
        result.StatusDescription.Should().Be("Shipped");
        result.SalesPersonName.Should().Be("Linda Mitchell");
        result.TerritoryName.Should().Be("Northwest");
        result.BillToAddress!.AddressLine1.Should().Be("123 Main St");
        result.BillToAddress.StateProvince.Should().Be("Washington");
        result.ShipToAddress!.AddressLine1.Should().Be("456 Oak Ave");
        result.LineItems.Should().HaveCount(1);
        result.LineItems[0].ProductName.Should().Be("Mountain-100 Silver, 38");
        result.LineItems[0].OrderQty.Should().Be(1);
        result.LineItems[0].UnitPrice.Should().Be(2024.994m);
        result.LineItems[0].LineTotal.Should().Be(2024.994m);
    }

    [Fact]
    public async Task Handle_returns_null_when_order_not_found()
    {
        // Arrange
        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderDetailAsync(999999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesOrderHeader?)null);

        // Act
        var result = await _sut.Handle(new ReadSalesOrderDetailQuery { SalesOrderId = 999999 }, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_null_salesperson_name_when_salesperson_not_assigned()
    {
        // Arrange
        var entity = new SalesOrderHeader
        {
            SalesOrderId = 43660,
            SalesOrderNumber = "SO43660",
            OrderDate = new DateTime(2011, 5, 31),
            DueDate = new DateTime(2011, 6, 12),
            Status = 1,
            BillToAddressEntity = new AddressEntity
            {
                AddressLine1 = "1 Test St",
                City = "Portland",
                PostalCode = "97201",
                StateProvince = new StateProvinceEntity { Name = "Oregon" }
            },
            ShipToAddressEntity = new AddressEntity
            {
                AddressLine1 = "1 Test St",
                City = "Portland",
                PostalCode = "97201",
                StateProvince = new StateProvinceEntity { Name = "Oregon" }
            },
            SalesPerson = null,
            TerritoryEntity = null,
            SalesOrderDetails = []
        };

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderDetailAsync(43660, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.Handle(new ReadSalesOrderDetailQuery { SalesOrderId = 43660 }, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.SalesPersonName.Should().BeNull();
        result.TerritoryName.Should().BeNull();
        result.LineItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_null_territory_name_when_territory_not_assigned()
    {
        // Arrange
        var entity = new SalesOrderHeader
        {
            SalesOrderId = 43661,
            SalesOrderNumber = "SO43661",
            OrderDate = new DateTime(2011, 5, 31),
            DueDate = new DateTime(2011, 6, 12),
            Status = 2,
            BillToAddressEntity = new AddressEntity
            {
                AddressLine1 = "2 Test St",
                City = "Portland",
                PostalCode = "97201",
                StateProvince = new StateProvinceEntity { Name = "Oregon" }
            },
            ShipToAddressEntity = new AddressEntity
            {
                AddressLine1 = "2 Test St",
                City = "Portland",
                PostalCode = "97201",
                StateProvince = new StateProvinceEntity { Name = "Oregon" }
            },
            SalesPerson = null,
            TerritoryEntity = null,
            SalesOrderDetails = []
        };

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderDetailAsync(43661, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.Handle(new ReadSalesOrderDetailQuery { SalesOrderId = 43661 }, CancellationToken.None);

        // Assert
        result!.TerritoryName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_maps_address_line2_as_null_when_not_present()
    {
        // Arrange
        var entity = new SalesOrderHeader
        {
            SalesOrderId = 43662,
            SalesOrderNumber = "SO43662",
            OrderDate = new DateTime(2011, 5, 31),
            DueDate = new DateTime(2011, 6, 12),
            Status = 5,
            BillToAddressEntity = new AddressEntity
            {
                AddressLine1 = "3 Test St",
                AddressLine2 = null,
                City = "Denver",
                PostalCode = "80201",
                StateProvince = new StateProvinceEntity { Name = "Colorado" }
            },
            ShipToAddressEntity = new AddressEntity
            {
                AddressLine1 = "3 Test St",
                AddressLine2 = null,
                City = "Denver",
                PostalCode = "80201",
                StateProvince = new StateProvinceEntity { Name = "Colorado" }
            },
            SalesPerson = null,
            TerritoryEntity = null,
            SalesOrderDetails = []
        };

        _mockSalesOrderRepository
            .Setup(x => x.GetSalesOrderDetailAsync(43662, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _sut.Handle(new ReadSalesOrderDetailQuery { SalesOrderId = 43662 }, CancellationToken.None);

        // Assert
        result!.BillToAddress!.AddressLine2.Should().BeNull();
        result.ShipToAddress!.AddressLine2.Should().BeNull();
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
