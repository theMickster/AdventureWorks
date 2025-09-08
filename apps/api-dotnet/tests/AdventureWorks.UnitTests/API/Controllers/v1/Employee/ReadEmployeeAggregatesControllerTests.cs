using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeAggregatesControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadEmployeeAggregatesController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadEmployeeAggregatesController _sut;

    public ReadEmployeeAggregatesControllerTests()
    {
        _sut = new ReadEmployeeAggregatesController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadEmployeeAggregatesController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadEmployeeAggregatesController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetAggregates_returns_ok_with_resultAsync()
    {
        var aggregatesModel = new EmployeeAggregatesModel
        {
            DepartmentHeadcounts = new List<DepartmentHeadcountSummaryModel>
            {
                new() { DepartmentId = (short)1, DepartmentName = "Engineering", GroupName = "Research and Development", ActiveEmployeeCount = 10 }
            }.AsReadOnly(),
            TenureDistribution = new TenureDistributionModel
            {
                UnderOneYear = 2,
                OneToThreeYears = 5,
                ThreeToFiveYears = 3,
                FiveToTenYears = 7,
                TenPlusYears = 1
            },
            PayBandSummary = new List<PayBandSummaryModel>
            {
                new() { DepartmentGroup = "Research and Development", AverageRate = 55.00m, MinRate = 40.00m, MaxRate = 70.00m }
            }.AsReadOnly()
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeAggregatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(aggregatesModel);

        var result = await _sut.GetAggregatesAsync();

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeSameAs(aggregatesModel);
        }
    }
}
