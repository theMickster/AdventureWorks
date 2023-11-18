using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Interfaces.Services.Address;
using AdventureWorks.Application.Services.Address;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Models;
using AdventureWorks.Domain.Models.Slim;
using AdventureWorks.Domain.Profiles;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Services.Address;

[ExcludeFromCodeCoverage]
public sealed class CreateAddressServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private readonly Mock<IValidator<AddressCreateModel>> _mockValidator = new();
    private CreateAddressService _sut;

    public CreateAddressServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressEntityToAddressModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new CreateAddressService(_mapper, _mockAddressRepository.Object, _mockValidator.Object!);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(CreateAddressService)
                .Should().Implement<ICreateAddressService>();

            typeof(CreateAddressService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new CreateAddressService(
                    null!,
                    _mockAddressRepository.Object,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new CreateAddressService(
                    _mapper,
                    null!,
                    _mockValidator.Object!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressRepository");

            _ = ((Action)(() => _sut = new CreateAddressService(
                    _mapper,
                    _mockAddressRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("validator");
        }
    }

    [Fact]
    public async Task CreateAsync_returns_successAsync()
    {
        var inputModel = new AddressCreateModel
        {
            AddressLine1 = "hello world",
            AddressLine2 = "hello world2",
            City = "Denver",
            PostalCode = "80256",
            AddressStateProvince = new GenericSlimModel {Id = 12}
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressCreateModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult{Errors = new List<ValidationFailure>()});

        _mockAddressRepository.Setup(x => x.AddAsync(It.IsAny<AddressEntity>()))
            .ReturnsAsync(new AddressEntity
            {
                AddressId = 8768,
                AddressLine1 = "hello world",
                AddressLine2 = "hello world2",
                City = "Denver",
                PostalCode = "80256",
                StateProvinceId = 1589,
                ModifiedDate = new DateTime(2011, 11, 11)
            });

        var (addressModel, errors) = await _sut.CreateAsync(inputModel);

        using (new AssertionScope())
        {
            addressModel.Should().NotBeNull();
            addressModel.Id.Should().Be(8768);
            errors.Count.Should().Be(0);
        }
    }

    [Fact]
    public void CreateAsync_throws_correct_exception()
    {
        _ = (((Func<Task>)(async () => await _sut.CreateAsync(null!)))
            .Should().ThrowAsync<ArgumentNullException>());
    }

    [Fact]
    public async Task CreateAsync_returns_correct_validation_errorsAsync()
    {
        var inputModel = new AddressCreateModel
        {
            AddressLine1 = "hello world"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<AddressCreateModel>(),
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

        var (addressModel, errors) = await _sut.CreateAsync(inputModel);

        using (new AssertionScope())
        {
            addressModel.Should().NotBeNull();
            addressModel.Id.Should().Be(0);
            errors.Count(x => x.ErrorMessage == "HelloWorld").Should().Be(1);
        }
    }
}