using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>Handles <see cref="ReadDepartmentHeadcountSummaryQuery"/> by returning all departments with their active employee counts, ordered by count descending.</summary>
public sealed class ReadDepartmentHeadcountSummaryQueryHandler(
    IDepartmentRepository departmentRepository)
        : IRequestHandler<ReadDepartmentHeadcountSummaryQuery, IReadOnlyList<DepartmentHeadcountSummaryModel>>
{
    private readonly IDepartmentRepository _repository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    public async Task<IReadOnlyList<DepartmentHeadcountSummaryModel>> Handle(ReadDepartmentHeadcountSummaryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var summaries = await _repository.GetDepartmentHeadcountSummaryAsync(cancellationToken);

        return summaries
            .Select(x => new DepartmentHeadcountSummaryModel
            {
                DepartmentId        = x.Dept.DepartmentId,
                DepartmentName      = x.Dept.Name,
                GroupName           = x.Dept.GroupName,
                ActiveEmployeeCount = x.Count
            })
            .ToList()
            .AsReadOnly();
    }
}
