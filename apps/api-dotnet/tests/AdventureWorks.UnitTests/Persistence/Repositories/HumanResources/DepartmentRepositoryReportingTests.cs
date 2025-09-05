using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Infrastructure.Persistence.Repositories.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Persistence.Repositories.HumanResources;

/// <summary>
/// EF InMemory integration tests for <see cref="DepartmentRepository"/> reporting methods.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class DepartmentRepositoryReportingTests : PersistenceUnitTestBase
{
    private readonly DepartmentRepository _sut;

    public DepartmentRepositoryReportingTests()
    {
        _sut = new DepartmentRepository(DbContext);

        // Seed departments
        DbContext.Departments.AddRange(
            new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "R&D", ModifiedDate = StandardModifiedDate },
            new DepartmentEntity { DepartmentId = 2, Name = "Marketing", GroupName = "Sales and Marketing", ModifiedDate = StandardModifiedDate });

        // Seed employees via navigation property (EF InMemory tracks related entity)
        DbContext.Employees.AddRange(
            HumanResourcesDomainFixtures.GetCompleteEmployeeEntity(1, "Alice", "Anderson"),  // active, CurrentFlag=true
            HumanResourcesDomainFixtures.GetCompleteEmployeeEntity(2, "Bob", "Brown"),        // active, CurrentFlag=true
            HumanResourcesDomainFixtures.GetCompleteEmployeeEntity(3, "Charlie", "Chen"),     // will be made inactive below
            HumanResourcesDomainFixtures.GetCompleteEmployeeEntity(4, "Diana", "Davis"));     // active, CurrentFlag=true

        // Employee 3: set inactive
        var emp3 = DbContext.Employees.Local.First(e => e.BusinessEntityId == 3);
        emp3.CurrentFlag = false;

        // Seed department history
        DbContext.EmployeeDepartmentHistories.AddRange(
            // Employee 1: active, open assignment in dept 1
            new EmployeeDepartmentHistory { BusinessEntityId = 1, DepartmentId = 1, ShiftId = 1, StartDate = new DateTime(2020, 1, 1), EndDate = null, ModifiedDate = StandardModifiedDate },
            // Employee 2: active, open assignment in dept 1
            new EmployeeDepartmentHistory { BusinessEntityId = 2, DepartmentId = 1, ShiftId = 1, StartDate = new DateTime(2020, 2, 1), EndDate = null, ModifiedDate = StandardModifiedDate },
            // Employee 3: inactive (CurrentFlag=false), open assignment in dept 1 — should NOT be counted
            new EmployeeDepartmentHistory { BusinessEntityId = 3, DepartmentId = 1, ShiftId = 1, StartDate = new DateTime(2020, 3, 1), EndDate = null, ModifiedDate = StandardModifiedDate },
            // Employee 4: active, closed assignment in dept 1 — should NOT be counted
            new EmployeeDepartmentHistory { BusinessEntityId = 4, DepartmentId = 1, ShiftId = 1, StartDate = new DateTime(2019, 1, 1), EndDate = new DateTime(2021, 1, 1), ModifiedDate = StandardModifiedDate });

        DbContext.SaveChanges();
    }

    [Fact]
    public async Task GetDepartmentHeadcountAsync_counts_only_active_employees_with_open_assignmentAsync()
    {
        // Dept 1: employee 1 and 2 are active+open. Employee 3 inactive. Employee 4 closed assignment.
        var count = await _sut.GetDepartmentHeadcountAsync(1);

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetDepartmentHeadcountAsync_returns_zero_for_department_with_no_active_assignmentsAsync()
    {
        // Dept 2 has no assignments at all
        var count = await _sut.GetDepartmentHeadcountAsync(2);

        count.Should().Be(0);
    }

    [Fact]
    public async Task GetDepartmentHeadcountSummaryAsync_includes_zero_count_departments_ordered_descAsync()
    {
        var result = await _sut.GetDepartmentHeadcountSummaryAsync();

        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            result[0].Dept.Name.Should().Be("Engineering");
            result[0].Count.Should().Be(2);
            result[1].Dept.Name.Should().Be("Marketing");
            result[1].Count.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetEmployeesByDepartmentAsync_returns_only_active_employees_with_open_assignmentAsync()
    {
        var (employees, totalCount) = await _sut.GetEmployeesByDepartmentAsync(1, 1, 20);

        using (new AssertionScope())
        {
            totalCount.Should().Be(2);
            employees.Should().HaveCount(2);
            employees.Should().NotContain(e => e.BusinessEntityId == 3); // inactive
            employees.Should().NotContain(e => e.BusinessEntityId == 4); // closed assignment
        }
    }

    [Fact]
    public async Task GetEmployeesByDepartmentAsync_paginates_correctlyAsync()
    {
        // Only 2 employees in dept 1; page 1 with pageSize 1 should return 1 record
        var (employees, totalCount) = await _sut.GetEmployeesByDepartmentAsync(1, 1, 1);

        using (new AssertionScope())
        {
            totalCount.Should().Be(2);
            employees.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task GetEmployeesByDepartmentAsync_returns_empty_for_department_with_no_active_employeesAsync()
    {
        // Dept 2 has no assignments
        var (employees, totalCount) = await _sut.GetEmployeesByDepartmentAsync(2, 1, 20);

        using (new AssertionScope())
        {
            totalCount.Should().Be(0);
            employees.Should().BeEmpty();
        }
    }
}
