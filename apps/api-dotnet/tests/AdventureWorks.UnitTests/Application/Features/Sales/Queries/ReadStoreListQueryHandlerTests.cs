using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadStoreListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockContactEntityRepository = new();
    private ReadStoreListQueryHandler _sut;

    public ReadStoreListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreListQueryHandler(_mapper, _mockStoreRepository.Object, _mockContactEntityRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreListQueryHandler(
                    null!,
                    _mockStoreRepository.Object,
                    _mockContactEntityRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreListQueryHandler(
                    _mapper,
                    null!,
                    _mockContactEntityRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new ReadStoreListQueryHandler(
                    _mapper,
                    _mockStoreRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("beceRepository");
        }
    }

    [Fact]
    public async Task Handle_list_returns_correct_null_resultAsync()
    {
        _mockStoreRepository.Setup(x => x.GetStoresAsync(It.IsAny<StoreParameter>()))
            .ReturnsAsync((null!, 0));

        var result = await _sut.Handle(new ReadStoreListQuery{Parameters = new StoreParameter()}, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();

            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_list_returns_correct_empty_resultAsync()
    {
        var readOnlyList = new List<StoreEntity>().AsReadOnly();
        _mockStoreRepository.Setup(x => x.GetStoresAsync(It.IsAny<StoreParameter>()))
            .ReturnsAsync((readOnlyList, 0));

        var result = await _sut.Handle(new ReadStoreListQuery { Parameters = new StoreParameter() }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();

            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_list_returns_valid_paged_model_Async()
    {
        _mockStoreRepository.Setup(x => x.GetStoresAsync(It.IsAny<StoreParameter>()))
            .ReturnsAsync((SalesDomainFixtures.GetMockStores().ToList(), 3));

        _mockContactEntityRepository.Setup(x => x.GetContactsByStoreIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(SalesDomainFixtures.GetMockContactEntities().ToList());
        var param = new StoreParameter { PageNumber = 1, OrderBy = "Name", PageSize = 30, SortOrder = "DESCENDING" };

        var pagedResult = await _sut.Handle(new ReadStoreListQuery { Parameters = param }, CancellationToken.None);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();

            pagedResult.PageNumber.Should().Be(1);
            pagedResult.PageSize.Should().Be(30);
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse();
            pagedResult.TotalPages.Should().Be(1);
            pagedResult.TotalRecords.Should().Be(3);

            pagedResult.Results.Should().HaveCount(3);

            var store01 = pagedResult.Results.FirstOrDefault(x => x.Id == 2534);
            var store02 = pagedResult.Results.FirstOrDefault(x => x.Id == 2535);

            store01!.Should().NotBeNull();

            store01!.StoreContacts.Should().HaveCount(2);
            store02!.StoreContacts.Should().HaveCount(3);
            store02!.StoreAddresses.Should().HaveCount(4);
        }
    }

    [Fact]
    public async Task Handle_search_returns_correct_null_resultAsync()
    {
        _mockStoreRepository.Setup(x => x.SearchStoresAsync(It.IsAny<StoreParameter>(), It.IsAny<StoreSearchModel>()))
            .ReturnsAsync((null!, 0));

        var result = await _sut.Handle(new ReadStoreListQuery { Parameters = new StoreParameter(), SearchModel = new StoreSearchModel()}, CancellationToken.None);
        
        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();

            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_search_returns_correct_empty_resultAsync()
    {
        var readOnlyList = new List<StoreEntity>().AsReadOnly();
        _mockStoreRepository.Setup(x => x.SearchStoresAsync(It.IsAny<StoreParameter>(), It.IsAny<StoreSearchModel>()))
            .ReturnsAsync((readOnlyList, 0));

        var result = await _sut.Handle(new ReadStoreListQuery { Parameters = new StoreParameter(), SearchModel = new StoreSearchModel() }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();

            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_search_returns_valid_paged_model_Async()
    {
        _mockStoreRepository.Setup(x => x.SearchStoresAsync(It.IsAny<StoreParameter>(), It.IsAny<StoreSearchModel>()))
            .ReturnsAsync((SalesDomainFixtures.GetMockStores().Where(x => x.BusinessEntityId == 2535).ToList(), 1));

        _mockContactEntityRepository.Setup(x => x.GetContactsByStoreIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(SalesDomainFixtures.GetMockContactEntities().ToList());

        var queryParam = new StoreParameter { PageNumber = 1, OrderBy = "Name", PageSize = 10, SortOrder = "ASC" };
        var searchParam = new StoreSearchModel { Id = 2535 };

        var pagedResult = await _sut.Handle(new ReadStoreListQuery { Parameters = queryParam, SearchModel = searchParam }, CancellationToken.None);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();

            pagedResult.PageNumber.Should().Be(1);
            pagedResult.PageSize.Should().Be(10);
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse();
            pagedResult.TotalPages.Should().Be(1);
            pagedResult.TotalRecords.Should().Be(1);

            pagedResult.Results.Should().HaveCount(1);

            var store02 = pagedResult.Results.FirstOrDefault(x => x.Id == 2535);

            store02!.StoreContacts.Should().HaveCount(3);
            store02!.StoreAddresses.Should().HaveCount(4);
        }
    }

}
