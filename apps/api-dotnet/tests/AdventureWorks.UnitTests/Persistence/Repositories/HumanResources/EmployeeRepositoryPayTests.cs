using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories.HumanResources;

/// <summary>
/// In-memory EF Core tests for <see cref="EmployeeRepository"/> pay history methods.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class EmployeeRepositoryPayTests : PersistenceUnitTestBase
{
    private readonly EmployeeRepository _sut;

    public EmployeeRepositoryPayTests()
    {
        _sut = new EmployeeRepository(DbContext);
    }

    [Fact]
    public async Task RecordPayChangeAsync_inserts_record_correctlyAsync()
    {
        var record = new EmployeePayHistory
        {
            BusinessEntityId = 1,
            RateChangeDate = StandardModifiedDate,
            Rate = 55.00m,
            PayFrequency = 2,
            ModifiedDate = StandardModifiedDate
        };

        await _sut.RecordPayChangeAsync(record);

        var inserted = DbContext.EmployeePayHistories
            .Single(x => x.BusinessEntityId == 1);

        using (new AssertionScope())
        {
            inserted.Rate.Should().Be(55.00m);
            inserted.PayFrequency.Should().Be(2);
            inserted.RateChangeDate.Should().Be(StandardModifiedDate);
        }
    }

    [Fact]
    public async Task GetEmployeePayHistoryAsync_returns_records_ordered_by_RateChangeDate_descendingAsync()
    {
        var older = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var newer = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        DbContext.EmployeePayHistories.AddRange(
            new EmployeePayHistory
            {
                BusinessEntityId = 1,
                RateChangeDate = older,
                Rate = 40.00m,
                PayFrequency = 1,
                ModifiedDate = StandardModifiedDate
            },
            new EmployeePayHistory
            {
                BusinessEntityId = 1,
                RateChangeDate = newer,
                Rate = 60.00m,
                PayFrequency = 2,
                ModifiedDate = StandardModifiedDate
            });
        await DbContext.SaveChangesAsync();

        var result = await _sut.GetEmployeePayHistoryAsync(1);

        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            result[0].RateChangeDate.Should().Be(newer);
            result[1].RateChangeDate.Should().Be(older);
        }
    }

    [Fact]
    public async Task GetEmployeePayHistoryAsync_returns_empty_list_for_employee_with_no_historyAsync()
    {
        var result = await _sut.GetEmployeePayHistoryAsync(999);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
