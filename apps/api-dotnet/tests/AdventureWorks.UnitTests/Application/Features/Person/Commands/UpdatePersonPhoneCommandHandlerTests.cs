using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Person.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdatePersonPhoneCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonPhoneRepository> _mockRepo = new();
    private readonly Mock<IValidator<PersonPhoneUpdateModel>> _mockValidator = new();
    private UpdatePersonPhoneCommandHandler _sut;

    public UpdatePersonPhoneCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(PersonPhoneEntityToPersonPhoneModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new UpdatePersonPhoneCommandHandler(_mapper, _mockRepo.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new UpdatePersonPhoneCommandHandler(null!, _mockRepo.Object, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new UpdatePersonPhoneCommandHandler(_mapper, null!, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personPhoneRepository");

            _ = ((Action)(() => _sut = new UpdatePersonPhoneCommandHandler(_mapper, _mockRepo.Object, null!)))
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
        var command = new UpdatePersonPhoneCommand
        {
            PersonId = 9999,
            PhoneNumberTypeId = 1,
            Model = new PersonPhoneUpdateModel { PhoneNumber = "555-9999" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_when_validator_fails()
    {
        var command = new UpdatePersonPhoneCommand
        {
            PersonId = 1,
            PhoneNumberTypeId = 1,
            Model = new PersonPhoneUpdateModel { PhoneNumber = string.Empty }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var failingValidator = new Setup.Fakes.FakeFailureValidator<PersonPhoneUpdateModel>("PhoneNumber", "bad");
        _sut = new UpdatePersonPhoneCommandHandler(_mapper, _mockRepo.Object, failingValidator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_tracked_phone_not_found()
    {
        var command = new UpdatePersonPhoneCommand
        {
            PersonId = 1,
            PhoneNumberTypeId = 99,
            Model = new PersonPhoneUpdateModel { PhoneNumber = "555-9999" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonPhoneUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.GetTrackedPhoneAsync(1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonPhone?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_calls_ReplacePhoneAsync_with_correct_args_and_returns_updated_model()
    {
        var command = new UpdatePersonPhoneCommand
        {
            PersonId = 1,
            PhoneNumberTypeId = 1,
            Model = new PersonPhoneUpdateModel { PhoneNumber = "555-9999" }
        };

        var existing = new PersonPhone
        {
            BusinessEntityId = 1,
            PhoneNumber = "555-1111",
            PhoneNumberTypeId = 1,
            ModifiedDate = DefaultAuditDate
        };

        var phoneType = new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = DefaultAuditDate };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonPhoneUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.GetTrackedPhoneAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.ReplacePhoneAsync(existing, "555-9999", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonPhone
            {
                BusinessEntityId = 1,
                PhoneNumber = "555-9999",
                PhoneNumberTypeId = 1,
                ModifiedDate = DefaultAuditDate,
                PhoneNumberType = phoneType
            });
        _mockRepo.Setup(x => x.GetPhoneWithDetailsByCompositeKeyAsync(1, "555-9999", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonPhone
            {
                BusinessEntityId = 1,
                PhoneNumber = "555-9999",
                PhoneNumberTypeId = 1,
                ModifiedDate = DefaultAuditDate,
                PhoneNumberType = phoneType
            });

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            _mockRepo.Verify(x => x.ReplacePhoneAsync(
                existing,
                "555-9999",
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            result.Should().BeOfType<PersonPhoneModel>();
            result.PhoneNumber.Should().Be("555-9999");
        }
    }
}
