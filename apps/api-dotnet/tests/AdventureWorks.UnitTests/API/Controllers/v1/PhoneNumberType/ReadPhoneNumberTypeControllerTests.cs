using AdventureWorks.API.Controllers.v1.PhoneNumberType;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Models.Features.Person;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.PhoneNumberType;

[ExcludeFromCodeCoverage]
public sealed class ReadPhoneNumberTypeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadPhoneNumberTypeController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadPhoneNumberTypeController _sut;

    public ReadPhoneNumberTypeControllerTests()
    {
        _sut = new ReadPhoneNumberTypeController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadPhoneNumberTypeController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadPhoneNumberTypeController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadPhoneNumberTypeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhoneNumberTypeModel { Id = 1, Name = "Cell" });

        var result = await _sut.GetByIdAsync(1);
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
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadPhoneNumberTypeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PhoneNumberTypeModel)null!);

        var result = await _sut.GetByIdAsync(999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate the phone number type.");
        }
    }

    [Fact]
    public async Task GetById_returns_bad_request_Async()
    {
        var result = await _sut.GetByIdAsync(0);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("A valid phone number type id must be specified.");
        }
    }

    [Fact]
    public async Task GetList_returns_ok_Async()
    {
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadPhoneNumberTypeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new() { Id = 1, Name = "Cell" },
                new() { Id = 2, Name = "Home" },
                new() { Id = 3, Name = "Work" }
            ]);

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
        _mockMediator.Setup(
                x => x.Send(It.IsAny<ReadPhoneNumberTypeListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PhoneNumberTypeModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            outputModel.Should().NotBeNull();
            outputModel!.Should().Be("Unable to locate records in the phone number type list.");
        }
    }
}
