using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeePayHistoryControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadEmployeePayHistoryController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadEmployeePayHistoryController _sut;

    public ReadEmployeePayHistoryControllerTests()
    {
        _sut = new ReadEmployeePayHistoryController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_when_logger_is_null()
    {
        ((Action)(() => _ = new ReadEmployeePayHistoryController(null!, _mockMediator.Object)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_throws_when_mediator_is_null()
    {
        ((Action)(() => _ = new ReadEmployeePayHistoryController(_mockLogger.Object, null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("mediator");
    }

    [Fact]
    public void Controller_has_Authorize_attribute()
    {
        var attribute = typeof(ReadEmployeePayHistoryController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .FirstOrDefault() as AuthorizeAttribute;

        attribute.Should().NotBeNull("because the controller must require authentication for all actions");
    }

    [Fact]
    public async Task EmployeeId_zero_returns_BadRequestAsync()
    {
        var result = await _sut.GetPayHistoryAsync(0, CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task EmployeeId_negative_returns_BadRequestAsync()
    {
        var result = await _sut.GetPayHistoryAsync(-5, CancellationToken.None);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The employee identifier must be a positive integer.");
        }
    }

    [Fact]
    public async Task KeyNotFoundException_propagates_not_caughtAsync()
    {
        const int employeeId = 999;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeePayHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Employee with ID {employeeId} not found."));

        Func<Task> act = async () => await _sut.GetPayHistoryAsync(employeeId, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Employee with ID {employeeId} not found.");
    }

    [Fact]
    public async Task Ok_with_history_list_on_successAsync()
    {
        const int employeeId = 1;

        var history = new List<EmployeePayHistoryModel>
        {
            new()
            {
                RateChangeDate = new DateTime(2025, 1, 1),
                Rate = 75.00m,
                PayFrequency = 2,
                PayFrequencyLabel = "Bi-Weekly"
            }
        }.AsReadOnly();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeePayHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _sut.GetPayHistoryAsync(employeeId, CancellationToken.None);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var resultList = okResult.Value as IReadOnlyList<EmployeePayHistoryModel>;
            resultList.Should().NotBeNull();
            resultList!.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task Ok_with_empty_list_on_no_historyAsync()
    {
        const int employeeId = 1;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeePayHistoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeePayHistoryModel>().AsReadOnly());

        var result = await _sut.GetPayHistoryAsync(employeeId, CancellationToken.None);

        var okResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var resultList = okResult.Value as IReadOnlyList<EmployeePayHistoryModel>;
            resultList.Should().NotBeNull();
            resultList!.Should().BeEmpty();
        }
    }
}
