using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Models.Slim;
using AdventureWorks.Test.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeAddressControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadEmployeeAddressController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadEmployeeAddressController _sut;

    public ReadEmployeeAddressControllerTests()
    {
        _sut = new ReadEmployeeAddressController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadEmployeeAddressController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadEmployeeAddressController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_returns_ok_with_addresses()
    {
        const int employeeId = 100;
        var addresses = new List<EmployeeAddressModel>
        {
            new()
            {
                AddressId = 1,
                AddressLine1 = "123 Main Street",
                City = "Seattle",
                PostalCode = "98101",
                StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" },
                AddressType = new GenericSlimModel { Id = 2, Name = "Home", Code = string.Empty }
            }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses.AsReadOnly());

        var result = await _sut.GetAllAsync(employeeId);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var resultList = objectResult.Value as IReadOnlyList<EmployeeAddressModel>;
            resultList.Should().NotBeNull();
            resultList!.Should().HaveCount(1);
            resultList[0].AddressId.Should().Be(1);
            resultList[0].City.Should().Be("Seattle");
        }
    }

    [Fact]
    public async Task GetAllAsync_returns_empty_list()
    {
        const int employeeId = 999;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeAddressModel>().AsReadOnly());

        var result = await _sut.GetAllAsync(employeeId);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var resultList = objectResult.Value as IReadOnlyList<EmployeeAddressModel>;
            resultList.Should().NotBeNull();
            resultList!.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetAllAsync_calls_mediator_with_correct_query()
    {
        const int employeeId = 274;
        ReadEmployeeAddressListQuery? capturedQuery = null;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IReadOnlyList<EmployeeAddressModel>>, CancellationToken>(
                (query, _) => capturedQuery = query as ReadEmployeeAddressListQuery)
            .ReturnsAsync(new List<EmployeeAddressModel>().AsReadOnly());

        await _sut.GetAllAsync(employeeId);

        using (new AssertionScope())
        {
            capturedQuery.Should().NotBeNull();
            capturedQuery!.BusinessEntityId.Should().Be(employeeId);

            _mockMediator.Verify(
                x => x.Send(It.IsAny<ReadEmployeeAddressListQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task GetAllAsync_logs_information()
    {
        const int employeeId = 100;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeAddressModel>
            {
                new() { AddressId = 1, AddressLine1 = "Test", City = "Test", PostalCode = "12345" }
            }.AsReadOnly());

        await _sut.GetAllAsync(employeeId);

        using (new AssertionScope())
        {
            _mockLogger.VerifyLoggingMessageContains(
                "Retrieving all addresses for employee",
                null,
                LogLevel.Information);

            _mockLogger.VerifyLoggingMessageContains(
                "Retrieved",
                null,
                LogLevel.Information);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public async Task GetAllAsync_handles_various_employee_ids(int employeeId)
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeAddressModel>().AsReadOnly());

        var result = await _sut.GetAllAsync(employeeId);
        var objectResult = result as OkObjectResult;

        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_returns_ok_when_address_found()
    {
        const int employeeId = 100;
        const int addressId = 1;

        var address = new EmployeeAddressModel
        {
            AddressId = addressId,
            AddressLine1 = "123 Main Street",
            City = "Seattle",
            PostalCode = "98101",
            StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" },
            AddressType = new GenericSlimModel { Id = 2, Name = "Home", Code = string.Empty }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        var result = await _sut.GetByIdAsync(employeeId, addressId);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var resultAddress = objectResult.Value as EmployeeAddressModel;
            resultAddress.Should().NotBeNull();
            resultAddress!.AddressId.Should().Be(addressId);
            resultAddress.City.Should().Be("Seattle");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_not_found_when_address_not_found()
    {
        const int employeeId = 100;
        const int addressId = 999;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeAddressModel?)null);

        var result = await _sut.GetByIdAsync(employeeId, addressId);
        var objectResult = result as NotFoundObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            objectResult.Value.Should().Be($"Address {addressId} not found for employee {employeeId}.");
        }
    }

    [Fact]
    public async Task GetByIdAsync_calls_mediator_with_correct_query()
    {
        const int employeeId = 274;
        const int addressId = 42;
        ReadEmployeeAddressQuery? capturedQuery = null;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<EmployeeAddressModel?>, CancellationToken>(
                (query, _) => capturedQuery = query as ReadEmployeeAddressQuery)
            .ReturnsAsync(new EmployeeAddressModel
            {
                AddressId = addressId,
                AddressLine1 = "Test",
                City = "Test",
                PostalCode = "12345"
            });

        await _sut.GetByIdAsync(employeeId, addressId);

        using (new AssertionScope())
        {
            capturedQuery.Should().NotBeNull();
            capturedQuery!.BusinessEntityId.Should().Be(employeeId);
            capturedQuery.AddressId.Should().Be(addressId);

            _mockMediator.Verify(
                x => x.Send(It.IsAny<ReadEmployeeAddressQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task GetByIdAsync_logs_information_when_found()
    {
        const int employeeId = 100;
        const int addressId = 1;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeAddressModel
            {
                AddressId = addressId,
                AddressLine1 = "Test",
                City = "Test",
                PostalCode = "12345"
            });

        await _sut.GetByIdAsync(employeeId, addressId);

        _mockLogger.VerifyLoggingMessageContains(
            "Retrieving address",
            null,
            LogLevel.Information);
    }

    [Fact]
    public async Task GetByIdAsync_logs_warning_when_not_found()
    {
        const int employeeId = 100;
        const int addressId = 999;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeAddressModel?)null);

        await _sut.GetByIdAsync(employeeId, addressId);

        _mockLogger.VerifyLoggingMessageContains(
            "Address",
            null,
            LogLevel.Warning);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 5)]
    [InlineData(500, 10)]
    [InlineData(9999, 99)]
    public async Task GetByIdAsync_handles_various_valid_ids(int employeeId, int addressId)
    {
        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeAddressModel
            {
                AddressId = addressId,
                AddressLine1 = "Test",
                City = "Test",
                PostalCode = "12345"
            });

        var result = await _sut.GetByIdAsync(employeeId, addressId);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var resultAddress = objectResult.Value as EmployeeAddressModel;
            resultAddress.Should().NotBeNull();
            resultAddress!.AddressId.Should().Be(addressId);
        }
    }

    #endregion
}
