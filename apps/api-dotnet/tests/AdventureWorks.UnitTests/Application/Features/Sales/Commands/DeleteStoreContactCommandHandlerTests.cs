using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class DeleteStoreContactCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockBeceRepository = new();
    private DeleteStoreContactCommandHandler _sut;

    public DeleteStoreContactCommandHandlerTests()
    {
        _sut = new DeleteStoreContactCommandHandler(_mockBeceRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _ = ((Action)(() => _sut = new DeleteStoreContactCommandHandler(null!)))
            .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityContactRepository");
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_contact_does_not_existAsync()
    {
        var command = new DeleteStoreContactCommand
        {
            StoreId = 2534,
            PersonId = 100,
            ContactTypeId = 11
        };

        _mockBeceRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityContactEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_deletes_contact_and_returns_unitAsync()
    {
        var command = new DeleteStoreContactCommand
        {
            StoreId = 2534,
            PersonId = 100,
            ContactTypeId = 11
        };

        var existing = new BusinessEntityContactEntity
        {
            BusinessEntityId = 2534,
            PersonId = 100,
            ContactTypeId = 11,
            ModifiedDate = DefaultAuditDate
        };

        _mockBeceRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockBeceRepository.Setup(x => x.DeleteAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(Unit.Value);
            _mockBeceRepository.Verify(x => x.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
