using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.Person.Commands;

[ExcludeFromCodeCoverage]
public sealed class DeletePersonPhoneCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IPersonPhoneRepository> _mockRepo = new();
    private DeletePersonPhoneCommandHandler _sut;

    public DeletePersonPhoneCommandHandlerTests()
    {
        _sut = new DeletePersonPhoneCommandHandler(_mockRepo.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _ = ((Action)(() => _sut = new DeletePersonPhoneCommandHandler(null!)))
            .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personPhoneRepository");
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
        var command = new DeletePersonPhoneCommand { PersonId = 9999, PhoneNumberTypeId = 1 };

        _mockRepo.Setup(x => x.PersonExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_tracked_phone_not_found()
    {
        var command = new DeletePersonPhoneCommand { PersonId = 1, PhoneNumberTypeId = 99 };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.GetTrackedPhoneAsync(1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonPhone?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_calls_DeletePhoneAsync_with_correct_args_and_returns_unit()
    {
        var command = new DeletePersonPhoneCommand { PersonId = 1, PhoneNumberTypeId = 1 };

        var existing = new PersonPhone
        {
            BusinessEntityId = 1,
            PhoneNumber = "555-1111",
            PhoneNumberTypeId = 1,
            ModifiedDate = DefaultAuditDate
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.GetTrackedPhoneAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.DeletePhoneAsync(1, 1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            _mockRepo.Verify(x => x.DeletePhoneAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
            result.Should().Be(Unit.Value);
        }
    }
}
