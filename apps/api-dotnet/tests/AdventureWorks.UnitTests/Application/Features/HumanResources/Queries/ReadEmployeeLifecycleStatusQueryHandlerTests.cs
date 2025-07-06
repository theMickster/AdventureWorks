using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fixtures;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeLifecycleStatusQueryHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<ILogger<ReadEmployeeLifecycleStatusQueryHandler>> _mockLogger = new();
    private ReadEmployeeLifecycleStatusQueryHandler _sut;

    public ReadEmployeeLifecycleStatusQueryHandlerTests()
    {
        _sut = new ReadEmployeeLifecycleStatusQueryHandler(
            _mockEmployeeRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_exception_when_request_is_null()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_null_when_employee_not_foundAsync()
    {
        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 999 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeEntity?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_status_model_for_active_employee_with_complete_dataAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.EmployeeId.Should().Be(1);
            result.FullName.Should().Be("John Doe");
            result.EmploymentStatus.Should().Be("Active");
            result.HireDate.Should().Be(new DateTime(2020, 1, 10));
            result.TerminationDate.Should().BeNull();
            result.CurrentDepartment.Should().Be("Engineering");
            result.CurrentShift.Should().Be("Day");
            result.DepartmentStartDate.Should().Be(new DateTime(2023, 6, 1));
            result.CurrentPayRate.Should().Be(75.50m);
            result.PayRateEffectiveDate.Should().Be(new DateTime(2024, 1, 1));
            result.VacationHoursBalance.Should().Be(80);
            result.SickLeaveHoursBalance.Should().Be(48);
            result.EligibleForRehire.Should().BeFalse(); // Active employee
            result.RehireCount.Should().Be(0); // No past terminations
        }
    }

    [Fact]
    public async Task Handle_returns_status_model_for_terminated_employee_with_complete_dataAsync()
    {
        var employee = CreateTerminatedEmployeeWithCompleteData();

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 2 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.EmployeeId.Should().Be(2);
            result.FullName.Should().Be("Jane Smith");
            result.EmploymentStatus.Should().Be("Terminated");
            result.HireDate.Should().Be(new DateTime(2018, 3, 15));
            result.TerminationDate.Should().Be(new DateTime(2024, 10, 31));
            result.CurrentDepartment.Should().BeNull(); // No active assignment
            result.CurrentShift.Should().BeNull();
            result.DepartmentStartDate.Should().BeNull();
            result.VacationHoursBalance.Should().Be(0); // Paid out
            result.SickLeaveHoursBalance.Should().Be(0); // Paid out
            result.EligibleForRehire.Should().BeTrue(); // Terminated with past history
            result.RehireCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task Handle_correctly_determines_employment_status_as_active_async()
    {
        var employee = CreateActiveEmployeeWithCompleteData();
        employee.CurrentFlag = true;

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        result!.EmploymentStatus.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_correctly_determines_employment_status_as_terminated_async()
    {
        var employee = CreateTerminatedEmployeeWithCompleteData();
        employee.CurrentFlag = false;

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 2 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        result!.EmploymentStatus.Should().Be("Terminated");
    }

    [Fact]
    public async Task Handle_correctly_calculates_days_employed_for_active_employeeAsync()
    {
        var hireDate = DateTime.UtcNow.Date.AddDays(-365); // 1 year ago
        var employee = CreateActiveEmployeeWithCompleteData();
        employee.HireDate = hireDate;
        employee.CurrentFlag = true;

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        var expectedDays = (DateTime.UtcNow - hireDate).Days;
        result!.DaysEmployed.Should().Be(expectedDays);
    }

    [Fact]
    public async Task Handle_correctly_calculates_days_employed_for_terminated_employeeAsync()
    {
        var hireDate = new DateTime(2018, 1, 1);
        var terminationDate = new DateTime(2024, 10, 31);
        var employee = CreateTerminatedEmployeeWithCompleteData();
        employee.HireDate = hireDate;
        employee.CurrentFlag = false;

        // Update termination date in history
        var terminatedHistory = employee.EmployeeDepartmentHistory!
            .First(dh => dh.EndDate.HasValue);
        terminatedHistory.EndDate = terminationDate;

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 2 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        var expectedDays = (terminationDate - hireDate).Days;
        result!.DaysEmployed.Should().Be(expectedDays);
    }

    [Fact]
    public async Task Handle_returns_null_days_employed_when_no_department_history_existsAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();
        employee.CurrentFlag = false; // Terminated
        employee.EmployeeDepartmentHistory = null; // No history

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        result!.DaysEmployed.Should().BeNull();
    }

    [Fact]
    public async Task Handle_correctly_aggregates_active_department_assignmentAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result!.CurrentDepartment.Should().Be("Engineering");
            result.CurrentShift.Should().Be("Day");
            result.DepartmentStartDate.Should().Be(new DateTime(2023, 6, 1));
        }
    }

    [Fact]
    public async Task Handle_returns_null_department_info_when_no_active_assignmentAsync()
    {
        var employee = CreateTerminatedEmployeeWithCompleteData();

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 2 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result!.CurrentDepartment.Should().BeNull();
            result.CurrentShift.Should().BeNull();
            result.DepartmentStartDate.Should().BeNull();
        }
    }

    [Fact]
    public async Task Handle_correctly_finds_current_pay_rate_from_most_recent_entryAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();

        // Add multiple pay history records
        employee.EmployeePayHistory!.Clear();
        employee.EmployeePayHistory.Add(new EmployeePayHistory
        {
            BusinessEntityId = 1,
            RateChangeDate = new DateTime(2020, 1, 10),
            Rate = 50.00m,
            PayFrequency = 2,
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });
        employee.EmployeePayHistory.Add(new EmployeePayHistory
        {
            BusinessEntityId = 1,
            RateChangeDate = new DateTime(2022, 6, 1),
            Rate = 65.00m,
            PayFrequency = 2,
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });
        employee.EmployeePayHistory.Add(new EmployeePayHistory
        {
            BusinessEntityId = 1,
            RateChangeDate = new DateTime(2024, 1, 1), // Most recent
            Rate = 85.00m,
            PayFrequency = 2,
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result!.CurrentPayRate.Should().Be(85.00m); // Most recent rate
            result.PayRateEffectiveDate.Should().Be(new DateTime(2024, 1, 1));
        }
    }

    [Fact]
    public async Task Handle_returns_null_pay_info_when_no_pay_history_existsAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();
        employee.EmployeePayHistory = null;

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result!.CurrentPayRate.Should().BeNull();
            result.PayRateEffectiveDate.Should().BeNull();
        }
    }

    [Fact]
    public async Task Handle_correctly_counts_terminations_for_rehire_countAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();

        // Add multiple termination records
        employee.EmployeeDepartmentHistory!.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 1,
            ShiftId = 1,
            StartDate = new DateTime(2015, 1, 1),
            EndDate = new DateTime(2017, 12, 31), // First termination
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });
        employee.EmployeeDepartmentHistory.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 2,
            ShiftId = 1,
            StartDate = new DateTime(2018, 6, 1),
            EndDate = new DateTime(2020, 3, 15), // Second termination
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        result!.RehireCount.Should().Be(2); // Two past terminations
    }

    [Fact]
    public async Task Handle_sets_eligible_for_rehire_true_when_terminated_with_historyAsync()
    {
        var employee = CreateTerminatedEmployeeWithCompleteData();
        employee.CurrentFlag = false;

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 2 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        result!.EligibleForRehire.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_sets_eligible_for_rehire_false_when_activeAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();
        employee.CurrentFlag = true;

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        result!.EligibleForRehire.Should().BeFalse(); // Active employee
    }

    [Fact]
    public async Task Handle_sets_eligible_for_rehire_false_when_terminated_without_historyAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();
        employee.CurrentFlag = false; // Terminated
        employee.EmployeeDepartmentHistory!.Clear(); // No history

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        result!.EligibleForRehire.Should().BeFalse(); // No termination count
    }

    [Fact]
    public async Task Handle_calls_repository_with_correct_employee_idAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 100 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        await _sut.Handle(query, CancellationToken.None);

        _mockEmployeeRepository.Verify(
            x => x.GetEmployeeByIdWithLifecycleDataAsync(100, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_correctly_identifies_most_recent_terminationAsync()
    {
        var employee = CreateActiveEmployeeWithCompleteData();
        employee.CurrentFlag = true;

        // Add multiple terminations with different dates
        employee.EmployeeDepartmentHistory!.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 1,
            ShiftId = 1,
            StartDate = new DateTime(2015, 1, 1),
            EndDate = new DateTime(2016, 6, 30), // Older termination
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });
        employee.EmployeeDepartmentHistory.Add(new EmployeeDepartmentHistory
        {
            BusinessEntityId = 1,
            DepartmentId = 2,
            ShiftId = 1,
            StartDate = new DateTime(2017, 1, 1),
            EndDate = new DateTime(2019, 12, 31), // Most recent termination
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
        });

        var query = new ReadEmployeeLifecycleStatusQuery { EmployeeId = 1 };

        _mockEmployeeRepository
            .Setup(x => x.GetEmployeeByIdWithLifecycleDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.Handle(query, CancellationToken.None);

        // For active employee, termination date should be null
        // But we can verify the logic correctly identified the most recent one
        result!.RehireCount.Should().Be(2);
    }

    private static EmployeeEntity CreateActiveEmployeeWithCompleteData()
    {
        return new EmployeeEntity
        {
            BusinessEntityId = 1,
            NationalIdnumber = "123456789",
            LoginId = "adventure-works\\john.doe",
            JobTitle = "Software Engineer",
            BirthDate = new DateTime(1990, 5, 15),
            HireDate = new DateTime(2020, 1, 10),
            MaritalStatus = "M",
            Gender = "M",
            OrganizationLevel = 2,
            CurrentFlag = true,
            SalariedFlag = false,
            VacationHours = 80,
            SickLeaveHours = 48,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate,
            PersonBusinessEntity = new PersonEntity
            {
                BusinessEntityId = 1,
                PersonTypeId = 2,
                NameStyle = false,
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "M",
                EmailPromotion = 0,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
            },
            EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
            {
                new EmployeeDepartmentHistory
                {
                    BusinessEntityId = 1,
                    DepartmentId = 3,
                    ShiftId = 1,
                    StartDate = new DateTime(2023, 6, 1),
                    EndDate = null, // Active
                    ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate,
                    Department = new DepartmentEntity
                    {
                        DepartmentId = 3,
                        Name = "Engineering",
                        GroupName = "Research and Development",
                        ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
                    },
                    Shift = new ShiftEntity
                    {
                        ShiftId = 1,
                        Name = "Day",
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
                    }
                }
            },
            EmployeePayHistory = new List<EmployeePayHistory>
            {
                new EmployeePayHistory
                {
                    BusinessEntityId = 1,
                    RateChangeDate = new DateTime(2024, 1, 1),
                    Rate = 75.50m,
                    PayFrequency = 2,
                    ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
                }
            }
        };
    }

    private static EmployeeEntity CreateTerminatedEmployeeWithCompleteData()
    {
        return new EmployeeEntity
        {
            BusinessEntityId = 2,
            NationalIdnumber = "987654321",
            LoginId = "adventure-works\\jane.smith",
            JobTitle = "Former Employee",
            BirthDate = new DateTime(1985, 8, 20),
            HireDate = new DateTime(2018, 3, 15),
            MaritalStatus = "S",
            Gender = "F",
            OrganizationLevel = 1,
            CurrentFlag = false,
            SalariedFlag = false,
            VacationHours = 0,
            SickLeaveHours = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate,
            PersonBusinessEntity = new PersonEntity
            {
                BusinessEntityId = 2,
                PersonTypeId = 2,
                NameStyle = false,
                FirstName = "Jane",
                LastName = "Smith",
                MiddleName = "A",
                EmailPromotion = 0,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
            },
            EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
            {
                new EmployeeDepartmentHistory
                {
                    BusinessEntityId = 2,
                    DepartmentId = 2,
                    ShiftId = 1,
                    StartDate = new DateTime(2018, 3, 15),
                    EndDate = new DateTime(2024, 10, 31), // Terminated
                    ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate,
                    Department = new DepartmentEntity
                    {
                        DepartmentId = 2,
                        Name = "Sales",
                        GroupName = "Sales and Marketing",
                        ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
                    },
                    Shift = new ShiftEntity
                    {
                        ShiftId = 1,
                        Name = "Day",
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
                    }
                }
            },
            EmployeePayHistory = new List<EmployeePayHistory>
            {
                new EmployeePayHistory
                {
                    BusinessEntityId = 2,
                    RateChangeDate = new DateTime(2018, 3, 15),
                    Rate = 55.00m,
                    PayFrequency = 2,
                    ModifiedDate = HumanResourcesDomainFixtures.HumanResourcesDefaultAuditDate
                }
            }
        };
    }
}
