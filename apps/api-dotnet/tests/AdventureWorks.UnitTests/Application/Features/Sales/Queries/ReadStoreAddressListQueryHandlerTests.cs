using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreAddressListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IBusinessEntityAddressRepository> _mockBusinessEntityAddressRepository = new();
    private ReadStoreAddressListQueryHandler _sut;

    public ReadStoreAddressListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreAddressListQueryHandler(_mapper, _mockBusinessEntityAddressRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreAddressListQueryHandler(
                    null!,
                    _mockBusinessEntityAddressRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreAddressListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("businessEntityAddressRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_store_not_foundAsync()
    {
        _mockBusinessEntityAddressRepository.Setup(x => x.GetAddressesByStoreIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessEntityAddressEntity>());

        var result = await _sut.Handle(new ReadStoreAddressListQuery { StoreId = 9999 }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_address_list_for_store_with_addressesAsync()
    {
        const int storeId = 2534;

        var entity = SalesDomainFixtures.GetMockStores().First(x => x.BusinessEntityId == storeId);
        var addresses = entity.StoreBusinessEntity.BusinessEntityAddresses.ToList();

        _mockBusinessEntityAddressRepository.Setup(x => x.GetAddressesByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        var result = await _sut.Handle(new ReadStoreAddressListQuery { StoreId = storeId }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            // Field-level assertions exercise the full nested-navigation chain in
            // BusinessEntityAddressEntityToStoreAddressModelProfile so a regression in
            // any flatten step (Address.*, Address.StateProvince.*, Address.StateProvince.CountryRegion.*)
            // fails this test instead of slipping through a count-only assertion.
            var first = result[0];
            first.StoreId.Should().Be(storeId);
            first.AddressTypeId.Should().Be(1);
            first.AddressTypeName.Should().Be("Home");
            first.AddressLine1.Should().Be("1234 Broadway Ave");
            first.City.Should().Be("Aurora");
            first.PostalCode.Should().Be("80015");
            first.StateProvinceName.Should().Be("Colorado");
            first.CountryRegionCode.Should().Be("US");
            first.CountryRegionName.Should().Be("United States of America");
        }
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_store_has_no_addressesAsync()
    {
        _mockBusinessEntityAddressRepository.Setup(x => x.GetAddressesByStoreIdAsync(1111, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BusinessEntityAddressEntity>());

        var result = await _sut.Handle(new ReadStoreAddressListQuery { StoreId = 1111 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
