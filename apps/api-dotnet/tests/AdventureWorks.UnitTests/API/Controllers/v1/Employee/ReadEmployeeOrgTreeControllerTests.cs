using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeOrgTreeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadEmployeeOrgTreeController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadEmployeeOrgTreeController _sut;

    public ReadEmployeeOrgTreeControllerTests()
    {
        _sut = new ReadEmployeeOrgTreeController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadEmployeeOrgTreeController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadEmployeeOrgTreeController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetOrgTree_returns_ok_with_empty_list_when_no_employees_existAsync()
    {
        var emptyList = new List<EmployeeOrgTreeItemModel>().AsReadOnly();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeOrgTreeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var result = await _sut.GetOrgTreeAsync();

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeEquivalentTo(emptyList);
        }
    }

    [Fact]
    public async Task GetOrgTree_returns_ok_when_employees_existAsync()
    {
        var orgTree = new List<EmployeeOrgTreeItemModel>
        {
            new()
            {
                EmployeeId = 1,
                FullName = "John Doe",
                JobTitle = "CEO",
                DepartmentName = "Executive",
                OrganizationLevel = 1,
                ParentEmployeeId = null
            }
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadEmployeeOrgTreeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgTree.AsReadOnly());

        var result = await _sut.GetOrgTreeAsync();

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeEquivalentTo(orgTree);
        }
    }
}
