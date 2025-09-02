using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreSalesPersonAssignmentListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IStoreSalesPersonHistoryRepository> _mockHistoryRepository = new();
    private ReadStoreSalesPersonAssignmentListQueryHandler _sut;

    public ReadStoreSalesPersonAssignmentListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(StoreSalesPersonHistoryEntityToStoreSalesPersonAssignmentModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreSalesPersonAssignmentListQueryHandler(
            _mapper,
            _mockStoreRepository.Object,
            _mockHistoryRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreSalesPersonAssignmentListQueryHandler(
                    null!,
                    _mockStoreRepository.Object,
                    _mockHistoryRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreSalesPersonAssignmentListQueryHandler(
                    _mapper,
                    null!,
                    _mockHistoryRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new ReadStoreSalesPersonAssignmentListQueryHandler(
                    _mapper,
                    _mockStoreRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("historyRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_store_does_not_existAsync()
    {
        _mockStoreRepository.Setup(x => x.ExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(
            new ReadStoreSalesPersonAssignmentListQuery { StoreId = 9999 },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_store_has_no_historyAsync()
    {
        const int storeId = 2534;

        _mockStoreRepository.Setup(x => x.ExistsAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockHistoryRepository.Setup(x => x.GetAssignmentsByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoreSalesPersonHistoryEntity>());

        var result = await _sut.Handle(
            new ReadStoreSalesPersonAssignmentListQuery { StoreId = storeId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_returns_mapped_assignment_list_when_history_existsAsync()
    {
        const int storeId = 2534;
        const int salesPersonId1 = 1;
        const int salesPersonId2 = 7;

        var historyEntities = new List<StoreSalesPersonHistoryEntity>
        {
            new()
            {
                BusinessEntityId = storeId,
                SalesPersonId = salesPersonId1,
                StartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2024, 5, 31, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = DefaultAuditDate,
                Rowguid = Guid.NewGuid()
            },
            new()
            {
                BusinessEntityId = storeId,
                SalesPersonId = salesPersonId2,
                StartDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = null,
                ModifiedDate = DefaultAuditDate,
                Rowguid = Guid.NewGuid()
            }
        };

        _mockStoreRepository.Setup(x => x.ExistsAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockHistoryRepository.Setup(x => x.GetAssignmentsByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(historyEntities);

        var result = await _sut.Handle(
            new ReadStoreSalesPersonAssignmentListQuery { StoreId = storeId },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(m => m.SalesPersonId == salesPersonId1);
            result.Should().Contain(m => m.SalesPersonId == salesPersonId2);
        }
    }
}
