using AdventureWorks.Application.Features.AddressManagement.Commands;
using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Slim;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.AddressManagement.Commands;

public sealed class CreateAddressCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IValidator<AddressCreateModel>> _mockValidator = new();
    private CreateAddressCommandHandler _sut;

    public CreateAddressCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressEntityToAddressModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new CreateAddressCommandHandler(_mapper, _mockAddressRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new CreateAddressCommandHandler(
                    null!,
                    _mockAddressRepository.Object,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new CreateAddressCommandHandler(
                    _mapper,
                    null!,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressRepository");

            _ = ((Action)(() => _sut = new CreateAddressCommandHandler(
                    _mapper,
                    _mockAddressRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public void Handle_throws_correct_exception()
    {
        _ = (((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>());
    }


    [Fact]
    public async Task Handle_returns_successAsync()
    {
        var command = new CreateAddressCommand
        {
            Model = new AddressCreateModel
            {
                AddressLine1 = "hello world",
                AddressLine2 = "hello world2",
                City = "Denver",
                PostalCode = "80256",
                StateProvince = new GenericSlimModel { Id = 12, Code = string.Empty, Name = string.Empty }
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressCreateModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = new List<ValidationFailure>() });

        _mockAddressRepository.Setup(x => x.AddAsync(It.IsAny<AddressEntity>()))
            .ReturnsAsync(new AddressEntity
            {
                AddressId = 8768,
                AddressLine1 = "hello world",
                AddressLine2 = "hello world2",
                City = "Denver",
                PostalCode = "80256",
                StateProvinceId = 1589,
                ModifiedDate = DefaultAuditDate
            });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(8768);
    }

    [Fact]
    public async Task Handle_throws_correct_validation_errorsAsync()
    {
        var command = new CreateAddressCommand
        {
            Model = new AddressCreateModel
            {
                AddressLine1 = "    ",
                AddressLine2 = "unit 1",
                City = string.Empty,
                PostalCode = string.Empty,
                StateProvince = new GenericSlimModel(){Name = string.Empty, Code = string.Empty}
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        var validator = new FakeFailureValidator<AddressCreateModel>("AddressLine1", "Street is required");

        _sut = new CreateAddressCommandHandler(_mapper, _mockAddressRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Street is required").Should().Be(1);
    }
}