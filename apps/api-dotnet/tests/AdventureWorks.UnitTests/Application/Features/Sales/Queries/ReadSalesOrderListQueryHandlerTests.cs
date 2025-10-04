using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.UnitTests.Setup.Fakes;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesOrderListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesOrderRepository> _mockSalesOrderRepository = new();
    private readonly Mock<IValidator<ReadSalesOrderListQuery>> _mockValidator = new();
    private ReadSalesOrderListQueryHandler _sut;

    public ReadSalesOrderListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(SalesOrderEntityToModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<ReadSalesOrderListQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _sut = new ReadSalesOrderListQueryHandler(_mapper, _mockSalesOrderRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_returns_success_with_results()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadSalesOrderListQuery { Parameters = parameters };

        var customerPerson = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" };
        var salesPersonEmployee = new EmployeeEntity { BusinessEntityId = 2, PersonBusinessEntity = new PersonEntity { FirstName = "Jane", LastName = "Smith" } };

        var salesOrderEntity = new SalesOrderHeader
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            Status = 5,
            TotalDue = 119961.7161m,
            CustomerEntity = new CustomerEntity { CustomerId = 1, Person = customerPerson },
            SalesPerson = new SalesPersonEntity { Employee = salesPersonEmployee }
        };

        _mockSalesOrderRepository.Setup(x =>
            x.GetSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { salesOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalRecords.Should().Be(1);
        result.Results.Should().HaveCount(1);
        result.Results![0].SalesOrderNumber.Should().Be("SO43659");
        result.Results[0].CustomerName.Should().Be("John Doe");
        result.Results[0].SalesPersonName.Should().Be("Jane Smith");
        result.Results[0].StatusDescription.Should().Be("Shipped");
    }

    [Fact]
    public async Task Handle_returns_success_with_empty_results()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadSalesOrderListQuery { Parameters = parameters };

        _mockSalesOrderRepository.Setup(x =>
            x.GetSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<SalesOrderHeader>().AsReadOnly(), 0));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalRecords.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_calls_search_when_search_model_provided()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new SalesOrderSearchModel
        {
            OrderDateFrom = new DateTime(2011, 1, 1),
            OrderDateTo = new DateTime(2011, 12, 31),
            Status = 5,
            SalesPersonId = 1,
            TerritoryId = 1
        };
        var query = new ReadSalesOrderListQuery { Parameters = parameters, SearchModel = searchModel };

        var customerPerson = new PersonEntity { BusinessEntityId = 1, FirstName = "John", LastName = "Doe" };
        var salesPersonEmployee = new EmployeeEntity { BusinessEntityId = 2, PersonBusinessEntity = new PersonEntity { FirstName = "Jane", LastName = "Smith" } };

        var salesOrderEntity = new SalesOrderHeader
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            Status = 5,
            TotalDue = 119961.7161m,
            CustomerEntity = new CustomerEntity { CustomerId = 1, Person = customerPerson },
            SalesPerson = new SalesPersonEntity { Employee = salesPersonEmployee }
        };

        _mockSalesOrderRepository.Setup(x =>
            x.SearchSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), It.IsAny<SalesOrderSearchModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { salesOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        _mockSalesOrderRepository.Verify(x =>
            x.SearchSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), searchModel, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_handles_null_salesperson()
    {
        // Arrange
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadSalesOrderListQuery { Parameters = parameters };

        var salesOrderEntity = new SalesOrderHeader
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            Status = 5,
            TotalDue = 119961.7161m,
            CustomerEntity = new CustomerEntity
            {
                CustomerId = 1,
                Person = new PersonEntity { FirstName = "John", LastName = "Doe" }
            },
            SalesPerson = null
        };

        _mockSalesOrderRepository.Setup(x =>
            x.GetSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { salesOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results![0].SalesPersonName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_preserves_total_records_when_page_is_out_of_range()
    {
        // Arrange — repo returns empty list but with a non-zero total (e.g. page 2 of 1-page result set)
        var parameters = new SalesOrderParameter { PageNumber = 2, PageSize = 10 };
        var query = new ReadSalesOrderListQuery { Parameters = parameters };

        _mockSalesOrderRepository.Setup(x =>
            x.GetSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<SalesOrderHeader>().AsReadOnly(), 5));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.TotalRecords.Should().Be(5);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_store_name_for_store_customer()
    {
        // Arrange — store customer: PersonId/Person null, StoreEntity populated. Should surface the store name in CustomerName.
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadSalesOrderListQuery { Parameters = parameters };

        var salesOrderEntity = new SalesOrderHeader
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            Status = 5,
            TotalDue = 119961.7161m,
            CustomerEntity = new CustomerEntity
            {
                CustomerId = 1,
                Person = null,
                StoreEntity = new StoreEntity { BusinessEntityId = 292, Name = "Next-Door Bike Store" }
            },
            SalesPerson = null
        };

        _mockSalesOrderRepository.Setup(x =>
            x.GetSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { salesOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results![0].CustomerName.Should().Be("Next-Door Bike Store");
    }

    [Fact]
    public async Task Handle_returns_empty_customer_name_when_customer_has_no_person_or_store()
    {
        // Arrange — degenerate customer row with neither Person nor StoreEntity. Resolver should return string.Empty, not the legacy "Unknown".
        var parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 };
        var query = new ReadSalesOrderListQuery { Parameters = parameters };

        var salesOrderEntity = new SalesOrderHeader
        {
            SalesOrderId = 43659,
            SalesOrderNumber = "SO43659",
            OrderDate = new DateTime(2011, 5, 31),
            Status = 5,
            TotalDue = 119961.7161m,
            CustomerEntity = new CustomerEntity { CustomerId = 1, Person = null, StoreEntity = null },
            SalesPerson = null
        };

        _mockSalesOrderRepository.Setup(x =>
            x.GetSalesOrdersAsync(It.IsAny<SalesOrderParameter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new[] { salesOrderEntity }.ToList().AsReadOnly(), 1));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Results![0].CustomerName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_query_is_invalid()
    {
        // Arrange — use FakeFailureValidator to simulate a validation failure (established pattern in this project)
        var handler = new ReadSalesOrderListQueryHandler(
            _mapper,
            _mockSalesOrderRepository.Object,
            new FakeFailureValidator<ReadSalesOrderListQuery>("Parameters", "Parameters cannot be null"));

        var query = new ReadSalesOrderListQuery { Parameters = new SalesOrderParameter { PageNumber = 1, PageSize = 10 } };

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
