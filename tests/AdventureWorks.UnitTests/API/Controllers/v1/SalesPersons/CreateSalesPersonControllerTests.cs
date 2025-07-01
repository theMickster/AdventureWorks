using AdventureWorks.API.Controllers.v1.SalesPersons;
using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Models.Slim;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.SalesPersons;

[ExcludeFromCodeCoverage]
public sealed class CreateSalesPersonControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateSalesPersonController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateSalesPersonController _sut;

    public CreateSalesPersonControllerTests()
    {
        _sut = new CreateSalesPersonController(_mockLogger.Object, _mockMediator.Object);
    }

    private static SalesPersonCreateModel GetValidModel()
    {
        return new SalesPersonCreateModel
        {
            FirstName = "Jane",
            LastName = "Smith",
            NationalIdNumber = "987654321",
            LoginId = "adventure-works\\jane.smith",
            JobTitle = "Sales Rep",
            BirthDate = new DateTime(1988, 8, 20),
            HireDate = new DateTime(2019, 3, 15),
            MaritalStatus = "M",
            Gender = "F",
            Phone = new SalesPersonPhoneCreateModel
            {
                PhoneNumber = "555-987-6543",
                PhoneNumberTypeId = 1
            },
            EmailAddress = "jane.smith@adventure-works.com",
            Address = new AddressCreateModel
            {
                AddressLine1 = "789 Sales Avenue",
                City = "Seattle",
                PostalCode = "98102",
                StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" }
            },
            AddressTypeId = 2,
            TerritoryId = 1,
            SalesQuota = 300000,
            Bonus = 5000,
            CommissionPct = 0.02m
        };
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateSalesPersonController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateSalesPersonController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mediator");
        }
    }

    [Fact]
    public async Task PostAsync_null_input_returns_bad_requestAsync()
    {
        var result = await _sut.PostAsync(null);

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            objectResult!.Value.Should().Be("The sales person input model cannot be null.");
        }
    }

    [Fact]
    public void PostAsync_invalid_input_handles_exception()
    {
        var input = GetValidModel();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateSalesPersonCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "FirstName", ErrorCode = "00010", ErrorMessage = "First name is required" } }));

        Func<Task> act = async () => await _sut.PostAsync(input);

        _ = act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PostAsync_valid_input_returns_createdAsync()
    {
        const int newSalesPersonId = 999;

        var salesPersonModel = new SalesPersonModel
        {
            Id = newSalesPersonId,
            FirstName = "John",
            LastName = "Doe",
            JobTitle = "Sales Representative",
            SalesQuota = 250000m,
            Bonus = 0m,
            CommissionPct = 0.012m,
            ModifiedDate = DateTime.UtcNow
        };

        var input = GetValidModel();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateSalesPersonCommand>(), CancellationToken.None))
            .ReturnsAsync(newSalesPersonId);

        _mockMediator
            .Setup(x => x.Send(It.IsAny<ReadSalesPersonQuery>(), CancellationToken.None))
            .ReturnsAsync(salesPersonModel);

        var result = await _sut.PostAsync(input);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult!.RouteName.Should().Be("GetSalesPersonById");

            var routeValues = createdResult!.RouteValues;
            routeValues.Should().ContainKey("salesPersonId");
            routeValues!["salesPersonId"].Should().Be(newSalesPersonId);
        }
    }
}
