using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadEmployeeAggregatesQueryHandler(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository)
        : IRequestHandler<ReadEmployeeAggregatesQuery, EmployeeAggregatesModel>
{
    private readonly IEmployeeRepository _employeeRepository =
        employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    public async Task<EmployeeAggregatesModel> Handle(
        ReadEmployeeAggregatesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var headcountSummary = await _departmentRepository.GetDepartmentHeadcountSummaryAsync(cancellationToken);
        var activeEmployees = await _employeeRepository.GetActiveEmployeesWithPayHistoryAsync(cancellationToken);

        var departmentHeadcounts = headcountSummary
            .Select(x => new DepartmentHeadcountSummaryModel
            {
                DepartmentId = x.Dept.DepartmentId,
                DepartmentName = x.Dept.Name,
                GroupName = x.Dept.GroupName,
                ActiveEmployeeCount = x.Count
            })
            .ToList()
            .AsReadOnly();

        var tenureDistribution = ComputeTenureDistribution(activeEmployees);
        var payBandSummary = ComputePayBandSummary(activeEmployees);

        return new EmployeeAggregatesModel
        {
            DepartmentHeadcounts = departmentHeadcounts,
            TenureDistribution = tenureDistribution,
            PayBandSummary = payBandSummary
        };
    }

    private static TenureDistributionModel ComputeTenureDistribution(
        IReadOnlyList<EmployeeEntity> employees)
    {
        var today = DateTime.UtcNow.Date;
        var model = new TenureDistributionModel();

        foreach (var e in employees)
        {
            var yearsOfService = (today - e.HireDate.Date).TotalDays / 365.25;

            if (yearsOfService < 1) model.UnderOneYear++;
            else if (yearsOfService < 3) model.OneToThreeYears++;
            else if (yearsOfService < 5) model.ThreeToFiveYears++;
            else if (yearsOfService < 10) model.FiveToTenYears++;
            else model.TenPlusYears++;
        }

        return model;
    }

    private static IReadOnlyList<PayBandSummaryModel> ComputePayBandSummary(
        IReadOnlyList<EmployeeEntity> employees)
    {
        return employees
            .Select(e => new
            {
                GroupName = e.EmployeeDepartmentHistory
                    .OrderByDescending(edh => edh.StartDate)
                    .FirstOrDefault()?.Department?.GroupName ?? "Unknown",
                CurrentRate = e.EmployeePayHistory
                    .OrderByDescending(ph => ph.RateChangeDate)
                    .FirstOrDefault()?.Rate
            })
            .Where(x => x.CurrentRate.HasValue)
            .GroupBy(x => x.GroupName)
            .Select(g => new PayBandSummaryModel
            {
                DepartmentGroup = g.Key,
                AverageRate = Math.Round(g.Average(x => x.CurrentRate!.Value), 2),
                MinRate = g.Min(x => x.CurrentRate!.Value),
                MaxRate = g.Max(x => x.CurrentRate!.Value)
            })
            .OrderBy(x => x.DepartmentGroup)
            .ToList()
            .AsReadOnly();
    }
}
