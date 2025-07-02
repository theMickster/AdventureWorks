using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeAddressQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private ReadEmployeeAddressQueryHandler _sut;

    public ReadEmployeeAddressQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeeAddressProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEmployeeAddressQueryHandler(_mapper, _mockEmployeeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadEmployeeAddressQueryHandler(null!, _mockEmployeeRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadEmployeeAddressQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("employeeRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_when_entity_not_found_Async()
    {
        const int businessEntityId = 999;
        const int addressId = 888;

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(businessEntityId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity?)null);

        var result = await _sut.Handle(
            new ReadEmployeeAddressQuery { BusinessEntityId = businessEntityId, AddressId = addressId },
            CancellationToken.None);

        result.Should().BeNull("because the entity was not found in the repository");
    }

    [Fact]
    public async Task Handle_throws_exception_when_request_is_null_Async()
    {
        await ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>("because request cannot be null");
    }

    [Fact]
    public async Task Handle_returns_valid_model_with_complete_data_Async()
    {
        const int businessEntityId = 100;
        const int addressId = 1;
        const int addressTypeId = 2;
        const string addressLine1 = "123 Main Street";
        const string addressLine2 = "Apt 4B";
        const string city = "Seattle";
        const int stateProvinceId = 79;
        const string stateProvinceName = "Washington";
        const string stateProvinceCode = "WA";
        const string postalCode = "98101";
        const string addressTypeName = "Home";
        var modifiedDate = DefaultAuditDate;

        var addressEntity = new AddressEntity
        {
            AddressId = addressId,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            StateProvinceId = stateProvinceId,
            PostalCode = postalCode,
            ModifiedDate = modifiedDate,
            StateProvince = new StateProvinceEntity
            {
                StateProvinceId = stateProvinceId,
                Name = stateProvinceName,
                StateProvinceCode = stateProvinceCode,
                CountryRegionCode = "US",
                CountryRegion = new CountryRegionEntity
                {
                    CountryRegionCode = "US",
                    Name = "United States"
                }
            }
        };

        var addressTypeEntity = new AddressTypeEntity
        {
            AddressTypeId = addressTypeId,
            Name = addressTypeName
        };

        var businessEntityAddressEntity = new BusinessEntityAddressEntity
        {
            BusinessEntityId = businessEntityId,
            AddressId = addressId,
            AddressTypeId = addressTypeId,
            Address = addressEntity,
            AddressType = addressTypeEntity,
            ModifiedDate = modifiedDate
        };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(businessEntityId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessEntityAddressEntity);

        var result = await _sut.Handle(
            new ReadEmployeeAddressQuery { BusinessEntityId = businessEntityId, AddressId = addressId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the entity was found");
            result!.AddressId.Should().Be(addressId, "because AddressId should map correctly");
            result.AddressLine1.Should().Be(addressLine1, "because AddressLine1 should map correctly");
            result.AddressLine2.Should().Be(addressLine2, "because AddressLine2 should map correctly");
            result.City.Should().Be(city, "because City should map correctly");
            result.PostalCode.Should().Be(postalCode, "because PostalCode should map correctly");
            result.ModifiedDate.Should().Be(modifiedDate, "because ModifiedDate should map correctly");

            result.StateProvince.Should().NotBeNull("because StateProvince should map correctly");
            result.StateProvince!.Id.Should().Be(stateProvinceId, "because StateProvince.Id should map correctly");
            result.StateProvince.Name.Should().Be(stateProvinceName, "because StateProvince.Name should map correctly");
            result.StateProvince.Code.Should().Be(stateProvinceCode, "because StateProvince.Code should map correctly");

            result.AddressType.Should().NotBeNull("because AddressType should map correctly");
            result.AddressType!.Id.Should().Be(addressTypeId, "because AddressType.Id should map correctly");
            result.AddressType.Name.Should().Be(addressTypeName, "because AddressType.Name should map correctly");
        }
    }

    [Theory]
    [InlineData(100, 1)]
    [InlineData(200, 2)]
    [InlineData(999, 888)]
    public async Task Handle_calls_repository_with_correct_ids(int businessEntityId, int addressId)
    {
        var entity = HumanResourcesDomainFixtures.GetValidBusinessEntityAddress(
            businessEntityId,
            addressId,
            2);

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(businessEntityId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _sut.Handle(
            new ReadEmployeeAddressQuery { BusinessEntityId = businessEntityId, AddressId = addressId },
            CancellationToken.None);

        _mockEmployeeRepository.Verify(
            x => x.GetEmployeeAddressByIdAsync(businessEntityId, addressId, It.IsAny<CancellationToken>()),
            Times.Once,
            "because the handler should call repository with the exact BusinessEntityId and AddressId from the query");
    }

    [Fact]
    public async Task Handle_maps_address_with_null_optional_fields_Async()
    {
        const int businessEntityId = 100;
        const int addressId = 1;

        var addressEntity = new AddressEntity
        {
            AddressId = addressId,
            AddressLine1 = "456 Oak Avenue",
            AddressLine2 = null, // Optional field
            City = "Portland",
            StateProvinceId = 79,
            PostalCode = "97201",
            ModifiedDate = DefaultAuditDate,
            StateProvince = new StateProvinceEntity
            {
                StateProvinceId = 79,
                Name = "Oregon",
                StateProvinceCode = "OR",
                CountryRegionCode = "US",
                CountryRegion = new CountryRegionEntity
                {
                    CountryRegionCode = "US",
                    Name = "United States"
                }
            }
        };

        var businessEntityAddressEntity = new BusinessEntityAddressEntity
        {
            BusinessEntityId = businessEntityId,
            AddressId = addressId,
            AddressTypeId = 2,
            Address = addressEntity,
            AddressType = new AddressTypeEntity
            {
                AddressTypeId = 2,
                Name = "Home"
            }
        };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressByIdAsync(businessEntityId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(businessEntityAddressEntity);

        var result = await _sut.Handle(
            new ReadEmployeeAddressQuery { BusinessEntityId = businessEntityId, AddressId = addressId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.AddressLine2.Should().BeNull("because AddressLine2 is optional");
        }
    }
}
