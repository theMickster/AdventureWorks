using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Person.Commands;

[ExcludeFromCodeCoverage]
public sealed class AddPersonPhoneCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonPhoneRepository> _mockRepo = new();
    private readonly Mock<IValidator<PersonPhoneCreateModel>> _mockValidator = new();
    private AddPersonPhoneCommandHandler _sut;

    public AddPersonPhoneCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(PersonPhoneCreateModelToPersonPhoneEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new AddPersonPhoneCommandHandler(_mapper, _mockRepo.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new AddPersonPhoneCommandHandler(null!, _mockRepo.Object, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new AddPersonPhoneCommandHandler(_mapper, null!, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personPhoneRepository");

            _ = ((Action)(() => _sut = new AddPersonPhoneCommandHandler(_mapper, _mockRepo.Object, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_person_does_not_exist()
    {
        var command = new AddPersonPhoneCommand
        {
            PersonId = 9999,
            Model = new PersonPhoneCreateModel { PhoneNumber = "555-1234", PhoneNumberTypeId = 1 }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_when_validator_fails()
    {
        var command = new AddPersonPhoneCommand
        {
            PersonId = 1,
            Model = new PersonPhoneCreateModel { PhoneNumber = string.Empty, PhoneNumberTypeId = 1 }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var failingValidator = new Setup.Fakes.FakeFailureValidator<PersonPhoneCreateModel>("PhoneNumber", "bad");
        _sut = new AddPersonPhoneCommandHandler(_mapper, _mockRepo.Object, failingValidator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule03_when_phone_number_type_not_found()
    {
        var command = new AddPersonPhoneCommand
        {
            PersonId = 1,
            Model = new PersonPhoneCreateModel { PhoneNumber = "555-1234", PhoneNumberTypeId = 999 }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonPhoneCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.PhoneNumberTypeExistsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule04_when_phone_combination_already_exists()
    {
        var command = new AddPersonPhoneCommand
        {
            PersonId = 1,
            Model = new PersonPhoneCreateModel { PhoneNumber = "555-1234", PhoneNumberTypeId = 1 }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonPhoneCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.PhoneNumberTypeExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.PhoneCombinationExistsAsync(1, "555-1234", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-04");
    }

    [Fact]
    public async Task Handle_persists_entity_with_correct_fields()
    {
        var command = new AddPersonPhoneCommand
        {
            PersonId = 1,
            Model = new PersonPhoneCreateModel { PhoneNumber = "555-1234", PhoneNumberTypeId = 1 }
        };

        var phoneType = new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = DefaultAuditDate };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonPhoneCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.PhoneNumberTypeExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.PhoneCombinationExistsAsync(1, "555-1234", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<PersonPhone>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonPhone e, CancellationToken _) => e);
        _mockRepo.Setup(x => x.GetPhoneWithDetailsByCompositeKeyAsync(1, "555-1234", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonPhone
            {
                BusinessEntityId = 1,
                PhoneNumber = "555-1234",
                PhoneNumberTypeId = 1,
                ModifiedDate = DefaultAuditDate,
                PhoneNumberType = phoneType
            });

        await _sut.Handle(command, CancellationToken.None);

        _mockRepo.Verify(x => x.AddAsync(
            It.Is<PersonPhone>(e =>
                e.BusinessEntityId == 1 &&
                e.PhoneNumber == "555-1234" &&
                e.ModifiedDate > DateTime.MinValue),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_PersonPhoneModel_with_correct_phone_number_and_type()
    {
        var command = new AddPersonPhoneCommand
        {
            PersonId = 1,
            Model = new PersonPhoneCreateModel { PhoneNumber = "555-1234", PhoneNumberTypeId = 1 }
        };

        var phoneType = new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = DefaultAuditDate };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonPhoneCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.PhoneNumberTypeExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.PhoneCombinationExistsAsync(1, "555-1234", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<PersonPhone>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonPhone e, CancellationToken _) => e);
        _mockRepo.Setup(x => x.GetPhoneWithDetailsByCompositeKeyAsync(1, "555-1234", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonPhone
            {
                BusinessEntityId = 1,
                PhoneNumber = "555-1234",
                PhoneNumberTypeId = 1,
                ModifiedDate = DefaultAuditDate,
                PhoneNumberType = phoneType
            });

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeOfType<PersonPhoneModel>();
            result.PhoneNumber.Should().Be("555-1234");
            result.PhoneNumberTypeId.Should().Be(1);
        }
    }
}
