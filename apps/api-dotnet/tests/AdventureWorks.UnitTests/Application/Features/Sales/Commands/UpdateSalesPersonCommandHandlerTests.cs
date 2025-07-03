using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

public sealed class UpdateSalesPersonCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private readonly Mock<IValidator<SalesPersonUpdateModel>> _mockValidator = new();
    private UpdateSalesPersonCommandHandler _sut;

    public UpdateSalesPersonCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(SalesPersonUpdateModelToSalesPersonEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new UpdateSalesPersonCommandHandler(_mapper, _mockSalesPersonRepository.Object, _mockValidator.Object);
    }

    private static SalesPersonUpdateModel GetValidUpdateModel()
    {
        return new SalesPersonUpdateModel
        {
            Id = 100,
            FirstName = "Jane",
            LastName = "Smith",
            MiddleName = "A.",
            Title = "Ms.",
            Suffix = "Jr.",
            JobTitle = "Senior Sales Rep",
            MaritalStatus = "M",
            Gender = "F",
            SalariedFlag = true,
            OrganizationLevel = 2,
            TerritoryId = 2,
            SalesQuota = 300000,
            Bonus = 2000,
            CommissionPct = 0.06m
        };
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_correct_exception()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None))).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_correct_validation_errorsAsync()
    {
        var model = GetValidUpdateModel();
        model.Id = 0; // Invalid ID

        var command = new UpdateSalesPersonCommand
        {
            Model = model,
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<SalesPersonUpdateModel>("Id", "Sales Person ID is required");

        _sut = new UpdateSalesPersonCommandHandler(_mapper, _mockSalesPersonRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Sales Person ID is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {
        // Build complete entity graph (Person → Employee → SalesPerson)
        var personEntity = new PersonEntity
        {
            BusinessEntityId = 100,
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "M.",
            Title = "Mr.",
            Suffix = "Sr."
        };

        var employeeEntity = new EmployeeEntity
        {
            BusinessEntityId = 100,
            JobTitle = "Sales Rep",
            MaritalStatus = "S",
            Gender = "M",
            SalariedFlag = false,
            OrganizationLevel = 1,
            PersonBusinessEntity = personEntity
        };

        var existingEntity = new SalesPersonEntity
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

        var command = new UpdateSalesPersonCommand
        {
            Model = GetValidUpdateModel(),
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<SalesPersonUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonByIdAsync(100))
            .ReturnsAsync(existingEntity);

        _mockSalesPersonRepository.Setup(x => x.UpdateSalesPersonWithEmployeeAsync(
                It.IsAny<SalesPersonEntity>(),
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
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
    }
}
