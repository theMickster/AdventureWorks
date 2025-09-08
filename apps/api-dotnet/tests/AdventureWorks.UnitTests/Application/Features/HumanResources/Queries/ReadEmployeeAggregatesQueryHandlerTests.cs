using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Test.Common.Extensions;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeAggregatesQueryHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository = new();
    private readonly ReadEmployeeAggregatesQueryHandler _sut;

    public ReadEmployeeAggregatesQueryHandlerTests()
    {
        _sut = new ReadEmployeeAggregatesQueryHandler(
            _mockEmployeeRepository.Object,
            _mockDepartmentRepository.Object);
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
    public async Task Handle_returns_aggregates_modelAsync()
    {
        SetupEmptyHeadcountSummary();
        SetupActiveEmployees(new List<EmployeeEntity>());

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.DepartmentHeadcounts.Should().NotBeNull();
            result.TenureDistribution.Should().NotBeNull();
            result.PayBandSummary.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Handle_correctly_buckets_tenure_under_one_yearAsync()
    {
        SetupEmptyHeadcountSummary();
        SetupActiveEmployees(new List<EmployeeEntity>
        {
            BuildEmployee(hireDate: DateTime.UtcNow.Date.AddDays(-180))
        });

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.TenureDistribution.UnderOneYear.Should().Be(1);
            result.TenureDistribution.OneToThreeYears.Should().Be(0);
            result.TenureDistribution.ThreeToFiveYears.Should().Be(0);
            result.TenureDistribution.FiveToTenYears.Should().Be(0);
            result.TenureDistribution.TenPlusYears.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_correctly_buckets_tenure_one_to_three_yearsAsync()
    {
        SetupEmptyHeadcountSummary();
        SetupActiveEmployees(new List<EmployeeEntity>
        {
            BuildEmployee(hireDate: DateTime.UtcNow.Date.AddYears(-2))
        });

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.TenureDistribution.UnderOneYear.Should().Be(0);
            result.TenureDistribution.OneToThreeYears.Should().Be(1);
            result.TenureDistribution.ThreeToFiveYears.Should().Be(0);
            result.TenureDistribution.FiveToTenYears.Should().Be(0);
            result.TenureDistribution.TenPlusYears.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_correctly_buckets_tenure_three_to_five_yearsAsync()
    {
        SetupEmptyHeadcountSummary();
        SetupActiveEmployees(new List<EmployeeEntity>
        {
            BuildEmployee(hireDate: DateTime.UtcNow.Date.AddYears(-4))
        });

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.TenureDistribution.UnderOneYear.Should().Be(0);
            result.TenureDistribution.OneToThreeYears.Should().Be(0);
            result.TenureDistribution.ThreeToFiveYears.Should().Be(1);
            result.TenureDistribution.FiveToTenYears.Should().Be(0);
            result.TenureDistribution.TenPlusYears.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_correctly_buckets_tenure_five_to_ten_yearsAsync()
    {
        SetupEmptyHeadcountSummary();
        SetupActiveEmployees(new List<EmployeeEntity>
        {
            BuildEmployee(hireDate: DateTime.UtcNow.Date.AddYears(-7))
        });

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.TenureDistribution.UnderOneYear.Should().Be(0);
            result.TenureDistribution.OneToThreeYears.Should().Be(0);
            result.TenureDistribution.ThreeToFiveYears.Should().Be(0);
            result.TenureDistribution.FiveToTenYears.Should().Be(1);
            result.TenureDistribution.TenPlusYears.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_correctly_buckets_tenure_ten_plus_yearsAsync()
    {
        SetupEmptyHeadcountSummary();
        SetupActiveEmployees(new List<EmployeeEntity>
        {
            BuildEmployee(hireDate: DateTime.UtcNow.Date.AddYears(-12))
        });

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.TenureDistribution.UnderOneYear.Should().Be(0);
            result.TenureDistribution.OneToThreeYears.Should().Be(0);
            result.TenureDistribution.ThreeToFiveYears.Should().Be(0);
            result.TenureDistribution.FiveToTenYears.Should().Be(0);
            result.TenureDistribution.TenPlusYears.Should().Be(1);
        }
    }

    [Fact]
    public async Task Handle_correctly_computes_pay_band_summaryAsync()
    {
        SetupEmptyHeadcountSummary();

        var employees = new List<EmployeeEntity>
        {
            BuildEmployee(groupName: "Research and Development", rate: 40.00m),
            BuildEmployee(groupName: "Research and Development", rate: 60.00m),
            BuildEmployee(groupName: "Sales and Marketing", rate: 30.00m)
        };

        SetupActiveEmployees(employees);

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.PayBandSummary.Count.Should().Be(2);

            var rd = result.PayBandSummary.First(x => x.DepartmentGroup == "Research and Development");
            rd.AverageRate.Should().Be(50.00m);
            rd.MinRate.Should().Be(40.00m);
            rd.MaxRate.Should().Be(60.00m);

            var sm = result.PayBandSummary.First(x => x.DepartmentGroup == "Sales and Marketing");
            sm.AverageRate.Should().Be(30.00m);
            sm.MinRate.Should().Be(30.00m);
            sm.MaxRate.Should().Be(30.00m);
        }
    }

    [Fact]
    public async Task Handle_excludes_employees_with_no_pay_history_from_pay_bandAsync()
    {
        SetupEmptyHeadcountSummary();

        var employees = new List<EmployeeEntity>
        {
            BuildEmployee(groupName: "Research and Development", rate: 50.00m),
            BuildEmployee(groupName: "Research and Development", rate: null)
        };

        SetupActiveEmployees(employees);

        var result = await _sut.Handle(new ReadEmployeeAggregatesQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.PayBandSummary.Count.Should().Be(1);
            result.PayBandSummary[0].DepartmentGroup.Should().Be("Research and Development");
            result.PayBandSummary[0].AverageRate.Should().Be(50.00m);
        }
    }

    private void SetupEmptyHeadcountSummary()
    {
        _mockDepartmentRepository
            .Setup(x => x.GetDepartmentHeadcountSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(DepartmentEntity Dept, int Count)>().AsReadOnly());
    }

    private void SetupActiveEmployees(IReadOnlyList<EmployeeEntity> employees)
    {
        _mockEmployeeRepository
            .Setup(x => x.GetActiveEmployeesWithPayHistoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);
    }

    private static EmployeeEntity BuildEmployee(
        DateTime? hireDate = null,
        string groupName = "Research and Development",
        decimal? rate = 50.00m)
    {
        var employee = new EmployeeEntity
        {
            BusinessEntityId = 1,
            NationalIdnumber = "123456789",
            LoginId = "adventure-works\\test.user",
            JobTitle = "Engineer",
            BirthDate = new DateTime(1985, 1, 1),
            HireDate = hireDate ?? DateTime.UtcNow.Date.AddYears(-2),
            MaritalStatus = "S",
            Gender = "M",
            CurrentFlag = true,
            SalariedFlag = false,
            VacationHours = 0,
            SickLeaveHours = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate,
            EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
            {
                new()
                {
                    DepartmentId = (short)1,
                    ShiftId = (byte)1,
                    StartDate = DateTime.UtcNow.Date.AddYears(-3),
                    EndDate = null,
                    ModifiedDate = DefaultAuditDate,
                    Department = new DepartmentEntity
                    {
                        DepartmentId = (short)1,
                        Name = "Engineering",
                        GroupName = groupName,
                        ModifiedDate = DefaultAuditDate
                    }
                }
            },
            EmployeePayHistory = rate.HasValue
                ? new List<EmployeePayHistory>
                {
                    new()
                    {
                        BusinessEntityId = 1,
                        RateChangeDate = DefaultAuditDate,
                        Rate = rate.Value,
                        PayFrequency = (byte)2,
                        ModifiedDate = DefaultAuditDate
                    }
                }
                : new List<EmployeePayHistory>()
        };

        return employee;
    }
}
