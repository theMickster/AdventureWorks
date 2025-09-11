using AdventureWorks.API.Controllers.v1.Persons;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Models.Features.Person;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Persons;

[ExcludeFromCodeCoverage]
public sealed class ReadPersonControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadPersonController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly ReadPersonController _sut;

    public ReadPersonControllerTests()
    {
        _sut = new ReadPersonController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadPersonController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadPersonController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task GetById_returns_ok_Async()
    {
        _mockMediator.Setup(x => x.Send(It.Is<ReadPersonQuery>(q => q.PersonId == 123), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDetailModel
            {
                BusinessEntityId = 123,
                FirstName = "Integration",
                LastName = "Person",
                PersonTypeName = "Employee"
            });

        var result = await _sut.GetByIdAsync(123);
        var objectResult = result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            objectResult.Value.Should().BeOfType<PersonDetailModel>();
        }
    }

    [Fact]
    public async Task GetById_returns_not_found_Async()
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<ReadPersonQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonDetailModel?)null);

        var result = await _sut.GetByIdAsync(9999);
        var objectResult = result as NotFoundObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            outputModel.Should().Be("Unable to locate the person.");
        }
    }

    [Fact]
    public async Task GetById_returns_bad_request_Async()
    {
        var result = await _sut.GetByIdAsync(-1);
        var objectResult = result as BadRequestObjectResult;
        var outputModel = objectResult!.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            outputModel.Should().Be("A valid person id must be specified.");
        }
    }
}
