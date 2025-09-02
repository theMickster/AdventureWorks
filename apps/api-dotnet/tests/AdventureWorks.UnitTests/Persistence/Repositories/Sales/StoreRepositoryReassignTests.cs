using AdventureWorks.Common.Constants;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

/// <summary>
/// In-memory EF Core tests for <see cref="StoreRepository.ReassignSalesPersonAsync"/>.
/// RepeatableRead isolation is not validated by in-memory (transactions are no-ops) but
/// all branch logic and entity state changes are exercised.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class StoreRepositoryReassignTests : PersistenceUnitTestBase
{
    private static readonly DateTime AssignedDate = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime StoreModifiedDate = new(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly StoreRepository _sut;

    public StoreRepositoryReassignTests()
    {
        _sut = new StoreRepository(DbContext);
        SeedData();
    }

    private void SeedData()
    {
        DbContext.BusinessEntities.AddRange(
            new() { BusinessEntityId = 5000, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new() { BusinessEntityId = 5001, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new() { BusinessEntityId = 5002, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new() { BusinessEntityId = 9001, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new() { BusinessEntityId = 9002, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new() { BusinessEntityId = 9003, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate }
        );

        DbContext.SalesPersons.AddRange(
            new SalesPersonEntity { BusinessEntityId = 9001, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new SalesPersonEntity { BusinessEntityId = 9002, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new SalesPersonEntity { BusinessEntityId = 9003, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate }
        );

        // Store 5000: has an open history record for SP 9001
        // Store 5001: no history record but has SalesPersonId = 9001 (SP exists → synthetic record expected)
        // Store 5002: no history record and SalesPersonId = 9099 (SP deleted, doesn't exist → no synthetic record)
        DbContext.Stores.AddRange(
            new StoreEntity { BusinessEntityId = 5000, Name = "Open Record Store", SalesPersonId = 9001, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new StoreEntity { BusinessEntityId = 5001, Name = "No History SP Exists Store", SalesPersonId = 9001, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate },
            new StoreEntity { BusinessEntityId = 5002, Name = "No History SP Missing Store", SalesPersonId = 9099, Rowguid = Guid.NewGuid(), ModifiedDate = StoreModifiedDate }
        );

        DbContext.StoreSalesPersonHistories.Add(new StoreSalesPersonHistoryEntity
        {
            BusinessEntityId = 5000,
            SalesPersonId = 9001,
            StartDate = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = null,
            ModifiedDate = StoreModifiedDate,
            Rowguid = Guid.NewGuid()
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public async Task ReassignSalesPersonAsync_closes_open_record_and_inserts_new_oneAsync()
    {
        const int storeId = 5000;
        const int newSpId = 9002;

        await _sut.ReassignSalesPersonAsync(storeId, newSpId, AssignedDate, CancellationToken.None);

        var history = DbContext.StoreSalesPersonHistories
            .Where(h => h.BusinessEntityId == storeId)
            .OrderBy(h => h.StartDate)
            .ToList();

        using (new AssertionScope())
        {
            history.Should().HaveCount(2);

            var closed = history.First(h => h.SalesPersonId == 9001);
            closed.EndDate.Should().Be(AssignedDate);

            var open = history.First(h => h.SalesPersonId == newSpId);
            open.EndDate.Should().BeNull();
            open.StartDate.Should().Be(AssignedDate);
        }
    }

    [Fact]
    public async Task ReassignSalesPersonAsync_creates_synthetic_outgoing_record_when_no_open_record_and_sp_existsAsync()
    {
        const int storeId = 5001;
        const int newSpId = 9002;

        await _sut.ReassignSalesPersonAsync(storeId, newSpId, AssignedDate, CancellationToken.None);

        var history = DbContext.StoreSalesPersonHistories
            .Where(h => h.BusinessEntityId == storeId)
            .OrderBy(h => h.StartDate)
            .ToList();

        using (new AssertionScope())
        {
            history.Should().HaveCount(2);

            var synthetic = history.First(h => h.SalesPersonId == 9001);
            synthetic.StartDate.Should().Be(StoreModifiedDate);
            synthetic.EndDate.Should().Be(AssignedDate);

            var open = history.First(h => h.SalesPersonId == newSpId);
            open.EndDate.Should().BeNull();
        }
    }

    [Fact]
    public async Task ReassignSalesPersonAsync_skips_synthetic_record_when_outgoing_sp_is_missingAsync()
    {
        const int storeId = 5002;
        const int newSpId = 9002;

        await _sut.ReassignSalesPersonAsync(storeId, newSpId, AssignedDate, CancellationToken.None);

        var history = DbContext.StoreSalesPersonHistories
            .Where(h => h.BusinessEntityId == storeId)
            .ToList();

        using (new AssertionScope())
        {
            history.Should().HaveCount(1);
            history.Single().SalesPersonId.Should().Be(newSpId);
            history.Single().EndDate.Should().BeNull();
        }
    }

    [Fact]
    public async Task ReassignSalesPersonAsync_throws_InvalidOperationException_when_same_person_reassignedAsync()
    {
        const int storeId = 5000;
        const int sameSpId = 9001;

        var act = async () => await _sut.ReassignSalesPersonAsync(storeId, sameSpId, AssignedDate, CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<InvalidOperationException>();
        assertion.Which.Message.Should().Be(SalesConstants.SameSalesPersonSentinel);
    }
}
