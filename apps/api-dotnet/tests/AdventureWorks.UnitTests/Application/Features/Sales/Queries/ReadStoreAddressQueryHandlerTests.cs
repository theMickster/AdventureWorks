using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreAddressQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IBusinessEntityAddressRepository> _mockBeaRepository = new();
    private ReadStoreAddressQueryHandler _sut;

    public ReadStoreAddressQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BusinessEntityAddressEntityToStoreAddressModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreAddressQueryHandler(_mapper, _mockBeaRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreAddressQueryHandler(null!, _mockBeaRepository.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreAddressQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityAddressRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_null_when_address_not_foundAsync()
    {
        var query = new ReadStoreAddressQuery { StoreId = 2534, AddressId = 100, AddressTypeId = 2 };

        _mockBeaRepository.Setup(x => x.GetWithDetailsByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityAddressEntity?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_mapped_modelAsync()
    {
        var query = new ReadStoreAddressQuery { StoreId = 2534, AddressId = 100, AddressTypeId = 2 };

        var hydrated = new BusinessEntityAddressEntity
        {
            BusinessEntityId = 2534,
            AddressId = 100,
            AddressTypeId = 2,
            ModifiedDate = DefaultAuditDate,
            AddressType = new AddressTypeEntity
            {
                AddressTypeId = 2,
                Name = "Main Office"
            },
            Address = new AddressEntity
            {
                AddressId = 100,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Suite 200",
                City = "Seattle",
                PostalCode = "98101",
                StateProvince = new StateProvinceEntity
                {
                    StateProvinceId = 79,
                    StateProvinceCode = "WA",
                    Name = "Washington",
                    CountryRegion = new CountryRegionEntity
                    {
                        CountryRegionCode = "US",
                        Name = "United States"
                    }
                }
            }
        };

        _mockBeaRepository.Setup(x => x.GetWithDetailsByCompositeKeyAsync(2534, 100, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hydrated);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(100);
            result!.StoreId.Should().Be(2534);
            result!.AddressTypeId.Should().Be(2);
            result!.AddressTypeName.Should().Be("Main Office");
            result!.AddressLine1.Should().Be("123 Main St");
            result!.AddressLine2.Should().Be("Suite 200");
            result!.City.Should().Be("Seattle");
            result!.PostalCode.Should().Be("98101");
            result!.StateProvinceCode.Should().Be("WA");
            result!.StateProvinceName.Should().Be("Washington");
            result!.CountryRegionCode.Should().Be("US");
            result!.CountryRegionName.Should().Be("United States");
            result!.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }
}
