using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Handles <see cref="ReadManufacturingKpisQuery"/> by delegating to the KPI repository.
/// </summary>
public sealed class ReadManufacturingKpisQueryHandler(IManufacturingKpiRepository manufacturingKpiRepository)
    : IRequestHandler<ReadManufacturingKpisQuery, ManufacturingKpisModel>
{
    private readonly IManufacturingKpiRepository _manufacturingKpiRepository = manufacturingKpiRepository
        ?? throw new ArgumentNullException(nameof(manufacturingKpiRepository));

    /// <summary>
    /// Retrieves aggregate manufacturing KPIs.
    /// </summary>
    /// <param name="request">the query (no parameters required)</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The aggregate manufacturing KPI model.</returns>
    public Task<ManufacturingKpisModel> Handle(ReadManufacturingKpisQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _manufacturingKpiRepository.GetManufacturingKpisAsync(cancellationToken);
    }
}
