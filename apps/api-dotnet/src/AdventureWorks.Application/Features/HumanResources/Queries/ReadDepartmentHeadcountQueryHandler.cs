using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>Handles <see cref="ReadDepartmentHeadcountQuery"/> by returning the active employee count for a single department.</summary>
public sealed class ReadDepartmentHeadcountQueryHandler(
    IDepartmentRepository departmentRepository)
        : IRequestHandler<ReadDepartmentHeadcountQuery, DepartmentHeadcountModel>
{
    private readonly IDepartmentRepository _repository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    public async Task<DepartmentHeadcountModel> Handle(ReadDepartmentHeadcountQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var dept = await _repository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (dept is null)
        {
            throw new KeyNotFoundException($"Department with ID {request.DepartmentId} not found.");
        }

        var count = await _repository.GetDepartmentHeadcountAsync(request.DepartmentId, cancellationToken);

        return new DepartmentHeadcountModel
        {
            DepartmentId = dept.DepartmentId,
            DepartmentName = dept.Name,
            ActiveEmployeeCount = count
        };
    }
}
