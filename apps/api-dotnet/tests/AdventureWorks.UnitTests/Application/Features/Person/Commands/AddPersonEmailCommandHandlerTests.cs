using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Person.Commands;

[ExcludeFromCodeCoverage]
public sealed class AddPersonEmailCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonEmailRepository> _mockRepo = new();
    private readonly Mock<IValidator<PersonEmailCreateModel>> _mockValidator = new();
    private AddPersonEmailCommandHandler _sut;

    public AddPersonEmailCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(PersonEmailCreateModelToEmailAddressEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new AddPersonEmailCommandHandler(_mapper, _mockRepo.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new AddPersonEmailCommandHandler(null!, _mockRepo.Object, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new AddPersonEmailCommandHandler(_mapper, null!, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personEmailRepository");

            _ = ((Action)(() => _sut = new AddPersonEmailCommandHandler(_mapper, _mockRepo.Object, null!)))
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
        var command = new AddPersonEmailCommand
        {
            PersonId = 9999,
            Model = new PersonEmailCreateModel { EmailAddress = "test@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_when_validator_fails()
    {
        var command = new AddPersonEmailCommand
        {
            PersonId = 1,
            Model = new PersonEmailCreateModel { EmailAddress = "bad-email" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var failingValidator = new Setup.Fakes.FakeFailureValidator<PersonEmailCreateModel>("EmailAddress", "bad");
        _sut = new AddPersonEmailCommandHandler(_mapper, _mockRepo.Object, failingValidator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule03_when_email_already_exists()
    {
        var command = new AddPersonEmailCommand
        {
            PersonId = 1,
            Model = new PersonEmailCreateModel { EmailAddress = "existing@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonEmailCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.EmailExistsForPersonAsync(1, "existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Handle_persists_entity_with_correct_fields()
    {
        var command = new AddPersonEmailCommand
        {
            PersonId = 1,
            Model = new PersonEmailCreateModel { EmailAddress = "new@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonEmailCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.EmailExistsForPersonAsync(1, "new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<EmailAddressEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailAddressEntity e, CancellationToken _) => e);

        await _sut.Handle(command, CancellationToken.None);

        _mockRepo.Verify(x => x.AddAsync(
            It.Is<EmailAddressEntity>(e =>
                e.BusinessEntityId == 1 &&
                e.EmailAddressName == "new@example.com" &&
                e.Rowguid != Guid.Empty &&
                e.ModifiedDate > DateTime.MinValue),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_returns_PersonEmailModel_with_db_generated_email_address_id()
    {
        var command = new AddPersonEmailCommand
        {
            PersonId = 1,
            Model = new PersonEmailCreateModel { EmailAddress = "new@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonEmailCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.EmailExistsForPersonAsync(1, "new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepo.Setup(x => x.AddAsync(It.IsAny<EmailAddressEntity>(), It.IsAny<CancellationToken>()))
            .Callback<EmailAddressEntity, CancellationToken>((e, _) => e.EmailAddressId = 5)
            .ReturnsAsync((EmailAddressEntity e, CancellationToken _) => e);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().BeOfType<PersonEmailModel>();
            result.EmailAddressId.Should().Be(5);
            result.EmailAddress.Should().Be("new@example.com");
        }
    }
}
