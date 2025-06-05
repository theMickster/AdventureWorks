using AdventureWorks.Application.Features.AddressManagement.Commands;
using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Slim;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.AddressManagement.Commands;

public sealed class UpdateAddressCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IValidator<AddressUpdateModel?>> _mockValidator = new();
    private UpdateAddressCommandHandler _sut;

    public UpdateAddressCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressUpdateModelToAddressEntityProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new UpdateAddressCommandHandler(_mapper, _mockAddressRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateAddressCommandHandler(
                    null!,
                    _mockAddressRepository.Object,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _ = new UpdateAddressCommandHandler(
                    _mapper,
                    null!,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressRepository");

            _ = ((Action)(() => _ = new UpdateAddressCommandHandler(
                    _mapper,
                    _mockAddressRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {
        var command = new UpdateAddressCommand()
        {
            Model = new AddressUpdateModel
            {
                Id = 12,
                AddressLine1 = "hello world",
                AddressLine2 = "hello world2",
                City = "Denver",
                PostalCode = "80256",
                StateProvince = new GenericSlimModel { Id = 12, Code = string.Empty, Name = string.Empty }
            },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressUpdateModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = new List<ValidationFailure>() });

        _mockAddressRepository.Setup(x => x.GetByIdAsync(12))
            .ReturnsAsync(new AddressEntity { AddressId = 12 });

        _mockAddressRepository.Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()));

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            _mockAddressRepository.Verify(x => x.GetByIdAsync(12), Times.Once);
            _mockAddressRepository.Verify(x => x.UpdateAsync(It.IsAny<AddressEntity>()), Times.Once);
        }
    }

    [Fact]
    public void Handle_throws_correct_exception()
    {
        _ = ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_returns_correct_validation_errorsAsync()
    {
        var command = new UpdateAddressCommand
        {
            Model = new AddressUpdateModel
            {
                AddressLine1 = "hello world"
            },
            ModifiedDate = DateTime.UtcNow
        };

        var validator = new FakeFailureValidator<AddressUpdateModel>("AddressLine2", "Street is required");

        _sut = new UpdateAddressCommandHandler(_mapper, _mockAddressRepository.Object, validator!);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Street is required").Should().Be(1);
    }
}