using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Handles <see cref="ReadManufacturingQualityScorecardQuery"/> by delegating to the scorecard repository.
/// </summary>
public sealed class ReadManufacturingQualityScorecardQueryHandler(
    IManufacturingQualityScorecardRepository qualityScorecardRepository)
    : IRequestHandler<ReadManufacturingQualityScorecardQuery, ManufacturingQualityScorecardModel>
{
    private readonly IManufacturingQualityScorecardRepository _qualityScorecardRepository =
        qualityScorecardRepository ?? throw new ArgumentNullException(nameof(qualityScorecardRepository));

    /// <summary>
    /// Retrieves the manufacturing quality scorecard.
    /// </summary>
    /// <param name="request">the scorecard query</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The manufacturing quality scorecard.</returns>
    public Task<ManufacturingQualityScorecardModel> Handle(
        ReadManufacturingQualityScorecardQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _qualityScorecardRepository.GetQualityScorecardAsync(cancellationToken);
    }
}
