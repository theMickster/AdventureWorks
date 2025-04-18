using AdventureWorks.Application.Features.AddressManagement.Contracts;
using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.AddressManagement.Services.Address;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Slim;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Services.Address;

[ExcludeFromCodeCoverage]
public sealed class UpdateAddressServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IValidator<AddressUpdateModel?>> _mockValidator = new();
    private readonly UpdateAddressService _sut;

    public UpdateAddressServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressUpdateModelToAddressEntityProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new UpdateAddressService(_mapper, _mockAddressRepository.Object, _mockValidator.Object);
    }


    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(UpdateAddressService)
                .Should().Implement<IUpdateAddressService>();

            typeof(UpdateAddressService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new UpdateAddressService(
                    null!,
                    _mockAddressRepository.Object,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _ = new UpdateAddressService(
                    _mapper,
                    null!,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressRepository");

            _ = ((Action)(() => _ = new UpdateAddressService(
                    _mapper,
                    _mockAddressRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    [SuppressMessage("Async", "AsyncifyInvocation:Use Task Async", Justification = "Because I Said so....")]
    public async Task UpdateAsync_returns_successAsync()
    {
        var inputModel = new AddressUpdateModel
        {
            Id = 12,
            AddressLine1 = "hello world",
            AddressLine2 = "hello world2",
            City = "Denver",
            PostalCode = "80256",
            AddressStateProvince = new GenericSlimModel { Id = 12, Code = string.Empty, Name = string.Empty }
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressUpdateModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = new List<ValidationFailure>() });

        _mockAddressRepository.Setup(x => x.GetByIdAsync(12))
            .ReturnsAsync(new AddressEntity { AddressId = 12 });

        _mockAddressRepository.Setup(x => x.UpdateAsync(It.IsAny<AddressEntity>()));

        var (addressModel, errors) = await _sut.UpdateAsync(inputModel);

        using (new AssertionScope())
        {
            addressModel.Should().NotBeNull();
            addressModel.Id.Should().Be(12);
            errors.Count.Should().Be(0);
        }
    }

    [Fact]
    public void UpdateAsync_throws_correct_exception()
    {
        _ = (((Func<Task>)(async () => await _sut.UpdateAsync(null!)))
            .Should().ThrowAsync<ArgumentNullException>());
    }

    [Fact]
    public async Task UpdateAsync_returns_correct_validation_errorsAsync()
    {
        var inputModel = new AddressUpdateModel
        {
            AddressLine1 = "hello world"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressUpdateModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult
            {
                Errors = new List<ValidationFailure>
                {
                    new()
                    {
                        ErrorCode = "ABC",
                        ErrorMessage = "HelloWorld"
                    }
                }
            });

        var (addressModel, errors) = await _sut.UpdateAsync(inputModel);

        using (new AssertionScope())
        {
            addressModel.Should().NotBeNull();
            addressModel.Id.Should().Be(0);
            errors.Count(x => x.ErrorMessage == "HelloWorld").Should().Be(1);
        }
    }
}