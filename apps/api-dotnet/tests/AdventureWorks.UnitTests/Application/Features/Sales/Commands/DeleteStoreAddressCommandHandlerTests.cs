using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

[ExcludeFromCodeCoverage]
public sealed class DeleteStoreAddressCommandHandlerTests : UnitTestBase
{
    private readonly Mock<IBusinessEntityAddressRepository> _mockBeaRepository = new();
    private DeleteStoreAddressCommandHandler _sut;

    public DeleteStoreAddressCommandHandlerTests()
    {
        _sut = new DeleteStoreAddressCommandHandler(_mockBeaRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _ = ((Action)(() => _sut = new DeleteStoreAddressCommandHandler(null!)))
            .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityAddressRepository");
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_address_does_not_existAsync()
    {
        var command = new DeleteStoreAddressCommand
        {
            StoreId = 2534,
            AddressId = 100,
            AddressTypeId = 2
        };

        _mockBeaRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_deletes_address_and_returns_unitAsync()
    {
        var command = new DeleteStoreAddressCommand
        {
            StoreId = 2534,
            AddressId = 100,
            AddressTypeId = 2
        };

        var existing = new BusinessEntityAddressEntity
        {
            BusinessEntityId = 2534,
            AddressId = 100,
            AddressTypeId = 2,
            ModifiedDate = DefaultAuditDate
        };

        _mockBeaRepository.Setup(x => x.GetByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _mockBeaRepository.Setup(x => x.DeleteAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().Be(Unit.Value);
            _mockBeaRepository.Verify(x => x.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
