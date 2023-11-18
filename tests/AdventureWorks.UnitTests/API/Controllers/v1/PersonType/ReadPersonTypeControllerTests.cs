using AdventureWorks.API.Controllers.v1.PersonType;
using AdventureWorks.Application.Interfaces.Services.PersonType;
using AdventureWorks.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using AdventureWorks.Domain.Models.Person;

namespace AdventureWorks.UnitTests.API.Controllers.v1.PersonType;

[ExcludeFromCodeCoverage]
public sealed class ReadPersonTypeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadPersonTypeController>> _mockLogger = new();
    private readonly Mock<IReadPersonTypeService> _mockReadPersonTypeService = new();
    private readonly ReadPersonTypeController _sut;

    public ReadPersonTypeControllerTests()
    {
        _sut = new ReadPersonTypeController(_mockLogger.Object, _mockReadPersonTypeService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadPersonTypeController(null!, _mockReadPersonTypeService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadPersonTypeController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readPersonTypeService");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockReadPersonTypeService.Setup(
                x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PersonTypeModel { Id = 1, Name = "Home", Code = "ABC", Description = "123456"});

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
        _mockReadPersonTypeService.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((PersonTypeModel)null!);

        var result = await _sut.GetByIdAsync(123456);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the person type.");
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
            outputModel!.Should().Be("A valid person type id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockReadPersonTypeService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(
                new List<PersonTypeModel>
                {
                    new() { Id = 1, Name = "Home"}
                    ,new() { Id = 2, Name = "Billing"}
                    ,new() { Id = 3, Name = "Mailing"}
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
        _mockReadPersonTypeService.Setup(
                x => x.GetListAsync())
            .ReturnsAsync(new List<PersonTypeModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records the person type list.");
        }
    }
}
