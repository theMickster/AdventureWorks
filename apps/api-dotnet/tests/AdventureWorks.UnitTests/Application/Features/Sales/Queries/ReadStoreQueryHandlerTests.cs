using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadStoreQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockContactEntityRepository = new();
    private ReadStoreQueryHandler _sut;

    public ReadStoreQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreQueryHandler(_mapper, _mockStoreRepository.Object, _mockContactEntityRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreQueryHandler(
                    null!,
                    _mockStoreRepository.Object,
                    _mockContactEntityRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreQueryHandler(
                    _mapper,
                    null!,
                    _mockContactEntityRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new ReadStoreQueryHandler(
                    _mapper,
                    _mockStoreRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("beceRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((StoreEntity)null!);

        var result = await _sut.Handle( new ReadStoreQuery{Id =22}, CancellationToken.None);

        result?.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_valid_model_Async()
    {
        const int storeId = 2534;

        var entity = SalesDomainFixtures.GetMockStores().FirstOrDefault(x => x.BusinessEntityId == storeId);

        var storeContacts =
            SalesDomainFixtures.GetMockContactEntities().Where(x => x.BusinessEntityId == storeId).ToList();

        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(storeId))
            .ReturnsAsync(entity);

        _mockContactEntityRepository.Setup(x => x.GetContactsByIdAsync(storeId))
            .ReturnsAsync(storeContacts);

        var result = await _sut.Handle(new ReadStoreQuery { Id = storeId }, CancellationToken.None);

        using (new AssertionScope())
        {
            result!.Should().NotBeNull();
            result!.Id.Should().Be(storeId);
            result.StoreAddresses.Should().HaveCount(2);
            result.ModifiedDate.Should().Be(DefaultAuditDate);

            result.StoreContacts.Should().HaveCount(2);

            result.StoreContacts.Count(x => x.FirstName == "Steve").Should().Be(1);
            result.StoreContacts.Count(x => x.LastName == "Jones").Should().Be(2);
        }
    }


}
