using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

public sealed class UpdateSalesPersonSalesConfigCommandHandlerTests : UnitTestBase
{
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private readonly Mock<IValidator<SalesPersonSalesConfigUpdateModel>> _mockValidator = new();
    private readonly Mock<IPublisher> _mockPublisher = new();
    private UpdateSalesPersonSalesConfigCommandHandler _sut;

    public UpdateSalesPersonSalesConfigCommandHandlerTests()
    {
        _sut = new UpdateSalesPersonSalesConfigCommandHandler(
            _mockSalesPersonRepository.Object,
            _mockValidator.Object,
            _mockPublisher.Object);
    }

    private static SalesPersonSalesConfigUpdateModel GetValidModel()
    {
        return new SalesPersonSalesConfigUpdateModel
        {
            Id = 100,
            TerritoryId = 2,
            SalesQuota = 300000,
            Bonus = 2000,
            CommissionPct = 0.12m
        };
    }

    private static SalesPersonEntity BuildEntityGraph()
    {
        var personEntity = new PersonEntity
        {
            BusinessEntityId = 100,
            FirstName = "John",
            LastName = "Doe"
        };

        var employeeEntity = new EmployeeEntity
        {
            BusinessEntityId = 100,
            JobTitle = "Sales Rep",
            MaritalStatus = "S",
            Gender = "M",
            SalariedFlag = false,
            PersonBusinessEntity = personEntity
        };

        return new SalesPersonEntity
        {
            BusinessEntityId = 100,
            CommissionPct = 0.03m,
            Bonus = 500,
            TerritoryId = 1,
            SalesQuota = 200000,
            ModifiedDate = DefaultAuditDate,
            Rowguid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc"),
            Employee = employeeEntity
        };
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_when_request_is_null()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None))).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_when_model_is_null()
    {
        var command = new UpdateSalesPersonSalesConfigCommand
        {
            Model = null!,
            ModifiedDate = DefaultAuditDate
        };

        var act = async () => await _sut.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_model_is_invalid()
    {
        var command = new UpdateSalesPersonSalesConfigCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<SalesPersonSalesConfigUpdateModel>("Id", "Sales Person ID must be greater than 0");
        _sut = new UpdateSalesPersonSalesConfigCommandHandler(
            _mockSalesPersonRepository.Object,
            validator,
            _mockPublisher.Object);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Sales Person ID must be greater than 0").Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_when_sales_person_does_not_exist()
    {
        var command = new UpdateSalesPersonSalesConfigCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<SalesPersonSalesConfigUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SalesPersonEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<KeyNotFoundException>();
        exceptionAssertion.Which.Message.Should().Contain("100");
    }

    [Fact]
    public async Task Handle_returns_success_and_calls_update_and_publishes_notification()
    {
        var command = new UpdateSalesPersonSalesConfigCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate,
            UserName = "test-user"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<SalesPersonSalesConfigUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildEntityGraph());

        SalesPersonEntity? capturedEntity = null;
        _mockSalesPersonRepository.Setup(x => x.UpdateSalesPersonWithEmployeeAsync(
                It.IsAny<SalesPersonEntity>(),
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Callback<SalesPersonEntity, EmployeeEntity, PersonEntity, DateTime, CancellationToken>(
                (sp, _, _, _, _) => capturedEntity = sp)
            .Returns(Task.CompletedTask);

        EntityChangedNotification? capturedNotification = null;
        _mockPublisher.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>(
                (n, _) => capturedNotification = n as EntityChangedNotification)
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockSalesPersonRepository.Verify(
            x => x.UpdateSalesPersonWithEmployeeAsync(
                It.IsAny<SalesPersonEntity>(),
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockPublisher.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);

        capturedEntity.Should().NotBeNull();
        capturedEntity!.TerritoryId.Should().Be(2);
        capturedEntity.SalesQuota.Should().Be(300000);
        capturedEntity.Bonus.Should().Be(2000);
        capturedEntity.CommissionPct.Should().Be(0.12m);

        capturedNotification.Should().NotBeNull();
        capturedNotification!.EntityType.Should().Be("SalesPerson");
        capturedNotification.EntityId.Should().Be(100);
        capturedNotification.Action.Should().Be("Updated");
        capturedNotification.UserName.Should().Be("test-user");
    }
}
