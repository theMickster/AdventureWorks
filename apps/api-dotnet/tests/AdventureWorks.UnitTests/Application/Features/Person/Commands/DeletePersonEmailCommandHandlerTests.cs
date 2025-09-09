using AdventureWorks.Application.Features.Person.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.Person.Commands;

[ExcludeFromCodeCoverage]
public sealed class DeletePersonEmailCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IPersonEmailRepository> _mockRepo = new();
    private DeletePersonEmailCommandHandler _sut;

    public DeletePersonEmailCommandHandlerTests()
    {
        _sut = new DeletePersonEmailCommandHandler(_mockRepo.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _ = ((Action)(() => _sut = new DeletePersonEmailCommandHandler(null!)))
            .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personEmailRepository");
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_email_does_not_exist()
    {
        var command = new DeletePersonEmailCommand { PersonId = 1, EmailAddressId = 99 };

        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailAddressEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_deletes_email_and_returns_unit()
    {
        var existing = new EmailAddressEntity
        {
            BusinessEntityId = 1,
            EmailAddressId = 2,
            EmailAddressName = "delete@example.com",
            ModifiedDate = DefaultAuditDate
        };

        var command = new DeletePersonEmailCommand { PersonId = 1, EmailAddressId = 2 };

        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockRepo.Setup(x => x.DeleteEmailAsync(1, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(Unit.Value);
            _mockRepo.Verify(x => x.DeleteEmailAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
