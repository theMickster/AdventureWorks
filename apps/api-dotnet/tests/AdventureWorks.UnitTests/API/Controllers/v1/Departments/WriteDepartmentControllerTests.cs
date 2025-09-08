using AdventureWorks.API.Controllers.v1.Departments;
using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Departments;

[ExcludeFromCodeCoverage]
public sealed class WriteDepartmentControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<WriteDepartmentController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly WriteDepartmentController _sut;

    public WriteDepartmentControllerTests()
    {
        _sut = new WriteDepartmentController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new WriteDepartmentController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new WriteDepartmentController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PostAsync_returns_bad_request_when_model_is_nullAsync()
    {
        var result = await _sut.PostAsync(null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The department input model cannot be null.");
        }
    }

    [Fact]
    public async Task PostAsync_returns_created_when_successfulAsync()
    {
        const short newDepartmentId = 42;

        var departmentModel = new DepartmentModel
        {
            Id = (short)newDepartmentId,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = DefaultAuditDate
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newDepartmentId);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentModel);

        var input = new DepartmentCreateModel { Name = "Engineering", GroupName = "Research and Development" };

        var result = await _sut.PostAsync(input);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult.RouteName.Should().Be("GetDepartmentById");

            var routeValues = createdResult.RouteValues;
            routeValues.Should().ContainKey("id");
            routeValues!["id"].Should().Be(newDepartmentId);

            var returnedModel = createdResult.Value as DepartmentModel;
            returnedModel.Should().NotBeNull();
            returnedModel!.Id.Should().Be((short)newDepartmentId);
            returnedModel.Name.Should().Be("Engineering");
        }
    }

    [Fact]
    public async Task PutAsync_returns_bad_request_when_model_is_nullAsync()
    {
        var result = await _sut.PutAsync(1, null);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The department input model cannot be null.");
        }
    }

    [Fact]
    public async Task PutAsync_returns_bad_request_when_id_does_not_match_model_idAsync()
    {
        var input = new DepartmentUpdateModel { Id = 5, Name = "Engineering", GroupName = "Research and Development" };

        var result = await _sut.PutAsync(99, input);

        var objectResult = result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult.Value.Should().Be("The ID in the route must match the ID in the request body.");
        }
    }

    [Fact]
    public async Task PutAsync_returns_ok_when_successfulAsync()
    {
        var input = new DepartmentUpdateModel { Id = 1, Name = "Engineering Updated", GroupName = "Research and Development" };

        var departmentModel = new DepartmentModel
        {
            Id = 1,
            Name = "Engineering Updated",
            GroupName = "Research and Development",
            ModifiedDate = DefaultAuditDate
        };

        _mockMediator
            .Setup(x => x.Send(It.IsAny<UpdateDepartmentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadDepartmentQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(departmentModel);

        var result = await _sut.PutAsync(1, input);

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);

            var returnedModel = objectResult.Value as DepartmentModel;
            returnedModel.Should().NotBeNull();
            returnedModel!.Name.Should().Be("Engineering Updated");
        }
    }
}
