using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.Person;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Person.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdatePersonEmailCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IPersonEmailRepository> _mockRepo = new();
    private readonly Mock<IValidator<PersonEmailUpdateModel>> _mockValidator = new();
    private UpdatePersonEmailCommandHandler _sut;

    public UpdatePersonEmailCommandHandlerTests()
    {
        _sut = new UpdatePersonEmailCommandHandler(_mockRepo.Object, _mockValidator.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new UpdatePersonEmailCommandHandler(null!, _mockValidator.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personEmailRepository");

            _ = ((Action)(() => _sut = new UpdatePersonEmailCommandHandler(_mockRepo.Object, null!)))
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
        var command = new UpdatePersonEmailCommand
        {
            PersonId = 9999,
            EmailAddressId = 1,
            Model = new PersonEmailUpdateModel { EmailAddress = "new@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_email_does_not_exist()
    {
        var command = new UpdatePersonEmailCommand
        {
            PersonId = 1,
            EmailAddressId = 99,
            Model = new PersonEmailUpdateModel { EmailAddress = "new@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonEmailUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailAddressEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_ValidationException_with_Rule03_when_new_email_already_exists_for_person()
    {
        var existing = new EmailAddressEntity
        {
            BusinessEntityId = 1,
            EmailAddressId = 1,
            EmailAddressName = "old@example.com",
            ModifiedDate = DefaultAuditDate
        };

        var command = new UpdatePersonEmailCommand
        {
            PersonId = 1,
            EmailAddressId = 1,
            Model = new PersonEmailUpdateModel { EmailAddress = "duplicate@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonEmailUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.EmailExistsForPersonAsync(1, "duplicate@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<ValidationException>();
        assertion.Which.Errors.Should().Contain(e => e.ErrorCode == "Rule-03");
    }

    [Fact]
    public async Task Handle_skips_duplicate_check_when_email_is_unchanged()
    {
        var existing = new EmailAddressEntity
        {
            BusinessEntityId = 1,
            EmailAddressId = 1,
            EmailAddressName = "same@example.com",
            ModifiedDate = DefaultAuditDate
        };

        var command = new UpdatePersonEmailCommand
        {
            PersonId = 1,
            EmailAddressId = 1,
            Model = new PersonEmailUpdateModel { EmailAddress = "same@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonEmailUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.UpdateAsync(It.IsAny<EmailAddressEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(1);
        _mockRepo.Verify(x => x.EmailExistsForPersonAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_updates_entity_and_returns_email_address_id()
    {
        var existing = new EmailAddressEntity
        {
            BusinessEntityId = 1,
            EmailAddressId = 2,
            EmailAddressName = "old@example.com",
            ModifiedDate = DefaultAuditDate
        };

        var command = new UpdatePersonEmailCommand
        {
            PersonId = 1,
            EmailAddressId = 2,
            Model = new PersonEmailUpdateModel { EmailAddress = "updated@example.com" }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<PersonEmailUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.EmailExistsForPersonAsync(1, "updated@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockRepo.Setup(x => x.UpdateAsync(It.IsAny<EmailAddressEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(2);
            existing.EmailAddressName.Should().Be("updated@example.com");
            existing.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
