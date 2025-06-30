using AdventureWorks.API.Controllers.v1.Employee;
using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Models.Slim;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.Employee;

[ExcludeFromCodeCoverage]
public sealed class CreateEmployeeControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<CreateEmployeeController>> _mockLogger = new();
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly CreateEmployeeController _sut;

    public CreateEmployeeControllerTests()
    {
        _sut = new CreateEmployeeController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new CreateEmployeeController(null!, _mockMediator.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new CreateEmployeeController(_mockLogger.Object, null!)))
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
            objectResult!.Value.Should().Be("The employee input model cannot be null.");
        }
    }

    [Fact]
    public void PostAsync_invalid_input_handles_exception()
    {
        var input = CreateValidEmployeeModel();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateEmployeeCommand>(), CancellationToken.None))
            .ThrowsAsync(new ValidationException(new List<ValidationFailure>
                { new() { PropertyName = "NationalIdNumber", ErrorCode = "Rule-15", ErrorMessage = "National ID number cannot be greater than 15 characters" } }));

        Func<Task> act = async () => await _sut.PostAsync(input);

        _ = act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PostAsync_valid_input_returns_createdAsync()
    {
        var input = CreateValidEmployeeModel();
        const int expectedBusinessEntityId = 100;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateEmployeeCommand>(), CancellationToken.None))
            .ReturnsAsync(expectedBusinessEntityId);

        var result = await _sut.PostAsync(input);

        var createdResult = result as CreatedAtRouteResult;

        using (new AssertionScope())
        {
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            createdResult!.RouteName.Should().Be("GetEmployeeById");

            var routeValues = createdResult!.RouteValues;
            routeValues.Should().NotBeNull();
            routeValues.Should().ContainKey("businessEntityId");
            routeValues!["businessEntityId"].Should().Be(expectedBusinessEntityId);

            var returnValue = createdResult.Value;
            returnValue.Should().NotBeNull();

            var businessEntityIdValue = returnValue.GetType()
                .GetProperty("businessEntityId")!
                .GetValue(returnValue, null);
            businessEntityIdValue.Should().Be(expectedBusinessEntityId);
        }
    }

    [Fact]
    public async Task PostAsync_valid_input_sends_command_with_correct_dataAsync()
    {
        var input = CreateValidEmployeeModel();
        CreateEmployeeCommand? capturedCommand = null;

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateEmployeeCommand>(), CancellationToken.None))
            .Callback<IRequest<int>, CancellationToken>((cmd, _) => capturedCommand = cmd as CreateEmployeeCommand)
            .ReturnsAsync(1);

        await _sut.PostAsync(input);

        using (new AssertionScope())
        {
            capturedCommand.Should().NotBeNull();
            capturedCommand!.Model.Should().BeSameAs(input);
            capturedCommand!.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            capturedCommand!.RowGuid.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task PostAsync_valid_input_logs_informationAsync()
    {
        var input = CreateValidEmployeeModel();

        _mockMediator
            .Setup(x => x.Send(It.IsAny<CreateEmployeeCommand>(), CancellationToken.None))
            .ReturnsAsync(1);

        await _sut.PostAsync(input);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating new employee")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Employee created successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PostAsync_null_input_logs_warningAsync()
    {
        await _sut.PostAsync(null);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateEmployee called with null input model")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static EmployeeCreateModel CreateValidEmployeeModel()
    {
        return new EmployeeCreateModel
        {
            // Person data
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Michael",
            Title = "Mr.",
            Suffix = "Jr.",

            // Employee data
            NationalIdNumber = "123456789",
            LoginId = "john.doe@adventure-works.com",
            JobTitle = "Software Engineer",
            BirthDate = new DateTime(1990, 5, 15),
            HireDate = new DateTime(2020, 1, 10),
            MaritalStatus = "M",
            Gender = "M",
            OrganizationLevel = 2,

            // Contact information
            Phone = new EmployeePhoneCreateModel
            {
                PhoneNumber = "555-123-4567",
                PhoneNumberTypeId = 1
            },
            EmailAddress = "john.doe@adventure-works.com",

            // Address information
            Address = new AddressCreateModel
            {
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Apt 4B",
                City = "Seattle",
                PostalCode = "98101",
                StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" }
            },
            AddressTypeId = 2
        };
    }
}
