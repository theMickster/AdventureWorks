using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeAddressListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private ReadEmployeeAddressListQueryHandler _sut;

    public ReadEmployeeAddressListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EmployeeAddressProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEmployeeAddressListQueryHandler(_mapper, _mockEmployeeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadEmployeeAddressListQueryHandler(null!, _mockEmployeeRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadEmployeeAddressListQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("employeeRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_exception_when_request_is_null_Async()
    {
        await ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>("because request cannot be null");
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_no_addresses_found_Async()
    {
        const int businessEntityId = 999;
        var emptyList = new List<BusinessEntityAddressEntity>().AsReadOnly();

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressesAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var result = await _sut.Handle(
            new ReadEmployeeAddressListQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        result.Should().NotBeNull("because the handler always returns a list");
        result.Should().BeEmpty("because no addresses were found for this employee");
    }

    [Fact]
    public async Task Handle_returns_valid_list_with_multiple_addresses_Async()
    {
        const int businessEntityId = 100;
        var modifiedDate = DefaultAuditDate;

        var addressEntities = new List<BusinessEntityAddressEntity>
        {
            new()
            {
                BusinessEntityId = businessEntityId,
                AddressId = 1,
                AddressTypeId = 2,
                Address = new AddressEntity
                {
                    AddressId = 1,
                    AddressLine1 = "123 Main Street",
                    AddressLine2 = "Apt 4B",
                    City = "Seattle",
                    StateProvinceId = 79,
                    PostalCode = "98101",
                    ModifiedDate = modifiedDate,
                    StateProvince = new StateProvinceEntity
                    {
                        StateProvinceId = 79,
                        Name = "Washington",
                        StateProvinceCode = "WA",
                        CountryRegionCode = "US",
                        CountryRegion = new CountryRegionEntity
                        {
                            CountryRegionCode = "US",
                            Name = "United States"
                        }
                    }
                },
                AddressType = new AddressTypeEntity
                {
                    AddressTypeId = 2,
                    Name = "Home"
                },
                ModifiedDate = modifiedDate
            },
            new()
            {
                BusinessEntityId = businessEntityId,
                AddressId = 2,
                AddressTypeId = 1,
                Address = new AddressEntity
                {
                    AddressId = 2,
                    AddressLine1 = "456 Oak Avenue",
                    AddressLine2 = null,
                    City = "Portland",
                    StateProvinceId = 79,
                    PostalCode = "97201",
                    ModifiedDate = modifiedDate,
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
                },
                AddressType = new AddressTypeEntity
                {
                    AddressTypeId = 1,
                    Name = "Billing"
                },
                ModifiedDate = modifiedDate
            }
        }.AsReadOnly();

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressesAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addressEntities);

        var result = await _sut.Handle(
            new ReadEmployeeAddressListQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull("because addresses were found");
            result.Should().HaveCount(2, "because two addresses were returned");

            var firstAddress = result.First();
            firstAddress.AddressId.Should().Be(1);
            firstAddress.AddressLine1.Should().Be("123 Main Street");
            firstAddress.AddressLine2.Should().Be("Apt 4B");
            firstAddress.City.Should().Be("Seattle");
            firstAddress.PostalCode.Should().Be("98101");
            firstAddress.StateProvince.Should().NotBeNull();
            firstAddress.StateProvince!.Name.Should().Be("Washington");
            firstAddress.AddressType.Should().NotBeNull();
            firstAddress.AddressType!.Name.Should().Be("Home");

            var secondAddress = result.Last();
            secondAddress.AddressId.Should().Be(2);
            secondAddress.AddressLine1.Should().Be("456 Oak Avenue");
            secondAddress.AddressLine2.Should().BeNull("because AddressLine2 is optional");
            secondAddress.City.Should().Be("Portland");
            secondAddress.AddressType!.Name.Should().Be("Billing");
        }
    }

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(999)]
    public async Task Handle_calls_repository_with_correct_business_entity_id(int businessEntityId)
    {
        var emptyList = new List<BusinessEntityAddressEntity>().AsReadOnly();

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressesAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        await _sut.Handle(
            new ReadEmployeeAddressListQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        _mockEmployeeRepository.Verify(
            x => x.GetEmployeeAddressesAsync(businessEntityId, It.IsAny<CancellationToken>()),
            Times.Once,
            "because the handler should call repository with the exact BusinessEntityId from the query");
    }

    [Fact]
    public async Task Handle_maps_all_address_fields_correctly_Async()
    {
        const int businessEntityId = 100;
        const int addressId = 1;
        const int addressTypeId = 2;
        const string addressLine1 = "789 Pine Road";
        const string addressLine2 = "Suite 300";
        const string city = "San Francisco";
        const int stateProvinceId = 6;
        const string stateProvinceName = "California";
        const string stateProvinceCode = "CA";
        const string postalCode = "94102";
        const string addressTypeName = "Main Office";
        var modifiedDate = DefaultAuditDate;

        var addressEntities = new List<BusinessEntityAddressEntity>
        {
            new()
            {
                BusinessEntityId = businessEntityId,
                AddressId = addressId,
                AddressTypeId = addressTypeId,
                Address = new AddressEntity
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
                },
                AddressType = new AddressTypeEntity
                {
                    AddressTypeId = addressTypeId,
                    Name = addressTypeName
                },
                ModifiedDate = modifiedDate
            }
        }.AsReadOnly();

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressesAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addressEntities);

        var result = await _sut.Handle(
            new ReadEmployeeAddressListQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            var address = result.First();

            address.AddressId.Should().Be(addressId, "because AddressId should map correctly");
            address.AddressLine1.Should().Be(addressLine1, "because AddressLine1 should map correctly");
            address.AddressLine2.Should().Be(addressLine2, "because AddressLine2 should map correctly");
            address.City.Should().Be(city, "because City should map correctly");
            address.PostalCode.Should().Be(postalCode, "because PostalCode should map correctly");
            address.ModifiedDate.Should().Be(modifiedDate, "because ModifiedDate should map correctly");

            address.StateProvince.Should().NotBeNull("because StateProvince should be mapped");
            address.StateProvince!.Id.Should().Be(stateProvinceId);
            address.StateProvince.Name.Should().Be(stateProvinceName);
            address.StateProvince.Code.Should().Be(stateProvinceCode);

            address.AddressType.Should().NotBeNull("because AddressType should be mapped");
            address.AddressType!.Id.Should().Be(addressTypeId);
            address.AddressType.Name.Should().Be(addressTypeName);
        }
    }

    [Fact]
    public async Task Handle_returns_list_in_correct_order_Async()
    {
        const int businessEntityId = 100;
        var modifiedDate = DefaultAuditDate;

        var addressEntities = new List<BusinessEntityAddressEntity>
        {
            new()
            {
                BusinessEntityId = businessEntityId,
                AddressId = 3,
                AddressTypeId = 1,
                Address = new AddressEntity
                {
                    AddressId = 3,
                    AddressLine1 = "Third Address",
                    City = "City3",
                    StateProvinceId = 79,
                    PostalCode = "30003",
                    ModifiedDate = modifiedDate,
                    StateProvince = new StateProvinceEntity
                    {
                        StateProvinceId = 79,
                        Name = "State",
                        StateProvinceCode = "ST",
                        CountryRegionCode = "US",
                        CountryRegion = new CountryRegionEntity { CountryRegionCode = "US", Name = "United States" }
                    }
                },
                AddressType = new AddressTypeEntity { AddressTypeId = 1, Name = "Type1" }
            },
            new()
            {
                BusinessEntityId = businessEntityId,
                AddressId = 1,
                AddressTypeId = 2,
                Address = new AddressEntity
                {
                    AddressId = 1,
                    AddressLine1 = "First Address",
                    City = "City1",
                    StateProvinceId = 79,
                    PostalCode = "10001",
                    ModifiedDate = modifiedDate,
                    StateProvince = new StateProvinceEntity
                    {
                        StateProvinceId = 79,
                        Name = "State",
                        StateProvinceCode = "ST",
                        CountryRegionCode = "US",
                        CountryRegion = new CountryRegionEntity { CountryRegionCode = "US", Name = "United States" }
                    }
                },
                AddressType = new AddressTypeEntity { AddressTypeId = 2, Name = "Type2" }
            },
            new()
            {
                BusinessEntityId = businessEntityId,
                AddressId = 2,
                AddressTypeId = 3,
                Address = new AddressEntity
                {
                    AddressId = 2,
                    AddressLine1 = "Second Address",
                    City = "City2",
                    StateProvinceId = 79,
                    PostalCode = "20002",
                    ModifiedDate = modifiedDate,
                    StateProvince = new StateProvinceEntity
                    {
                        StateProvinceId = 79,
                        Name = "State",
                        StateProvinceCode = "ST",
                        CountryRegionCode = "US",
                        CountryRegion = new CountryRegionEntity { CountryRegionCode = "US", Name = "United States" }
                    }
                },
                AddressType = new AddressTypeEntity { AddressTypeId = 3, Name = "Type3" }
            }
        }.AsReadOnly();

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeAddressesAsync(businessEntityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addressEntities);

        var result = await _sut.Handle(
            new ReadEmployeeAddressListQuery { BusinessEntityId = businessEntityId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().HaveCount(3);
            result[0].AddressId.Should().Be(3, "because order should be preserved from repository");
            result[1].AddressId.Should().Be(1);
            result[2].AddressId.Should().Be(2);
        }
    }
}
