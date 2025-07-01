using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Models.Slim;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

public sealed class CreateSalesPersonCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private readonly Mock<IValidator<SalesPersonCreateModel>> _mockValidator = new();
    private CreateSalesPersonCommandHandler _sut;

    public CreateSalesPersonCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(SalesPersonCreateModelToSalesPersonEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateSalesPersonCommandHandler(_mapper, _mockSalesPersonRepository.Object, _mockValidator.Object);
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
        var command = new CreateSalesPersonCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        var validator = new FakeFailureValidator<SalesPersonCreateModel>("FirstName", "First name is required");

        _sut = new CreateSalesPersonCommandHandler(_mapper, _mockSalesPersonRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "First name is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {
        var command = new CreateSalesPersonCommand
        {
            Model = GetValidModel(),
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<SalesPersonCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockSalesPersonRepository.Setup(x => x.CreateSalesPersonWithEmployeeAsync(
                It.IsAny<SalesPersonEntity>(),
                It.IsAny<EmployeeEntity>(),
                It.IsAny<PersonEntity>(),
                It.IsAny<PersonPhone>(),
                It.IsAny<EmailAddressEntity>(),
                It.IsAny<AddressEntity>(),
                It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(100);
    }
}
