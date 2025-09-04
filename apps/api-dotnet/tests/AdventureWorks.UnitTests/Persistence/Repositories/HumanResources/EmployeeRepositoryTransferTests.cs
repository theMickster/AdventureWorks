using AdventureWorks.Application.Exceptions;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories.HumanResources;

/// <summary>
/// In-memory EF Core tests for <see cref="EmployeeRepository.TransferEmployeeDepartmentAsync"/>.
/// Transactions are no-ops in InMemory, but all branch logic and entity mutations are exercised.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class EmployeeRepositoryTransferTests : PersistenceUnitTestBase
{
    private static readonly DateTime Today = new(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Yesterday = Today.AddDays(-1);

    private readonly EmployeeRepository _sut;

    public EmployeeRepositoryTransferTests()
    {
        _sut = new EmployeeRepository(DbContext);
    }

    [Fact]
    public async Task TransferEmployeeDepartmentAsync_throws_conflict_when_record_with_same_key_exists_on_transfer_dateAsync()
    {
        // Seed: a duplicate target record already on the transfer date
        DbContext.EmployeeDepartmentHistories.AddRange(
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = 1,
                DepartmentId = 1,
                ShiftId = 1,
                StartDate = Yesterday,
                EndDate = null,
                ModifiedDate = StandardModifiedDate
            },
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = 1,
                DepartmentId = 2,
                ShiftId = 1,
                StartDate = Today,
                EndDate = null,
                ModifiedDate = StandardModifiedDate
            });
        await DbContext.SaveChangesAsync();

        Func<Task> act = async () => await _sut.TransferEmployeeDepartmentAsync(
            businessEntityId: 1,
            newDepartmentId: 2,
            newShiftId: 1,
            transferDate: Today,
            modifiedDate: StandardModifiedDate);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task TransferEmployeeDepartmentAsync_closes_active_record_and_inserts_new_record_on_successAsync()
    {
        // Seed: one open record
        DbContext.EmployeeDepartmentHistories.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 1,
            ShiftId = 1,
            StartDate = Yesterday,
            EndDate = null,
            ModifiedDate = StandardModifiedDate
        });
        await DbContext.SaveChangesAsync();

        await _sut.TransferEmployeeDepartmentAsync(
            businessEntityId: 1,
            newDepartmentId: 2,
            newShiftId: 1,
            transferDate: Today,
            modifiedDate: StandardModifiedDate);

        var allRecords = DbContext.EmployeeDepartmentHistories
            .Where(dh => dh.BusinessEntityId == 1)
            .ToList();

        using (new AssertionScope())
        {
            allRecords.Should().HaveCount(2);

            var closedRecord = allRecords.Single(dh => dh.DepartmentId == 1);
            closedRecord.EndDate.Should().Be(Today);

            var newRecord = allRecords.Single(dh => dh.DepartmentId == 2);
            newRecord.EndDate.Should().BeNull();
            newRecord.StartDate.Should().Be(Today);
        }
    }

    [Fact]
    public async Task TransferEmployeeDepartmentAsync_throws_conflict_when_no_active_record_existsAsync()
    {
        // No open record seeded for employee 1 — only a closed one
        DbContext.EmployeeDepartmentHistories.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 1,
            ShiftId = 1,
            StartDate = Yesterday.AddDays(-30),
            EndDate = Yesterday,
            ModifiedDate = StandardModifiedDate
        });
        await DbContext.SaveChangesAsync();

        Func<Task> act = async () => await _sut.TransferEmployeeDepartmentAsync(
            businessEntityId: 1,
            newDepartmentId: 2,
            newShiftId: 1,
            transferDate: Today,
            modifiedDate: StandardModifiedDate);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*has no active department assignment*");
    }
}
