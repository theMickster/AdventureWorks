using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreAddressListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private ReadStoreAddressListQueryHandler _sut;

    public ReadStoreAddressListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreAddressListQueryHandler(_mapper, _mockStoreRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreAddressListQueryHandler(
                    null!,
                    _mockStoreRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreAddressListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");
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
        _mockStoreRepository.Setup(x => x.GetAddressesByStoreIdAsync(It.IsAny<int>()))
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

        _mockStoreRepository.Setup(x => x.GetAddressesByStoreIdAsync(storeId))
            .ReturnsAsync(addresses);

        var result = await _sut.Handle(new ReadStoreAddressListQuery { StoreId = storeId }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_store_has_no_addressesAsync()
    {
        _mockStoreRepository.Setup(x => x.GetAddressesByStoreIdAsync(1111))
            .ReturnsAsync(new List<BusinessEntityAddressEntity>());

        var result = await _sut.Handle(new ReadStoreAddressListQuery { StoreId = 1111 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
