using AdventureWorks.API.Controllers.v1.ContactType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Application.Features.HumanResources.Contracts;

namespace AdventureWorks.UnitTests.API.Controllers.v1.ContactType;

[ExcludeFromCodeCoverage]
public sealed class ReadContactTypeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadContactTypeController>> _mockLogger = new();
    private readonly Mock<IReadContactTypeService> _mockReadContactTypeService = new();
    private readonly ReadContactTypeController _sut;

    public ReadContactTypeControllerTests()
    {
        _sut = new ReadContactTypeController(_mockLogger.Object, _mockReadContactTypeService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadContactTypeController(null!, _mockReadContactTypeService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadContactTypeController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readContactTypeService");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockReadContactTypeService.Setup(
                x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ContactTypeModel { Id = 1, Name = "Home", Code  = string.Empty, Description = string.Empty});

        var result = await _sut.GetByIdAsync(123);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockReadContactTypeService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ContactTypeModel)null!);

        var result = await _sut.GetByIdAsync(123456);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the contact type.");
        }
    }

    [Fact]
    public async Task GetById_returns_bad_request_Async()
    {
        var result = await _sut.GetByIdAsync(-100);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid contact type id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockReadContactTypeService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(
                new List<ContactTypeModel>
                {
                    new() { Id = 1, Name = "Home", Code  = string.Empty, Description = string.Empty}
                    ,new() {Id = 2, Name = "Billing", Code = string.Empty, Description = string.Empty}
                    ,new() {Id = 3, Name = "Mailing", Code = string.Empty, Description = string.Empty}
                });

        var result = await _sut.GetListAsync();

        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetList_returns_not_found_Async()
    {
        _mockReadContactTypeService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(new List<ContactTypeModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records the contact type list.");
        }
    }
}
