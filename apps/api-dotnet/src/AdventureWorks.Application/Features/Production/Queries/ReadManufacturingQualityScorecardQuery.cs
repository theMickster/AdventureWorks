using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query that returns the manufacturing quality scorecard.
/// </summary>
public sealed class ReadManufacturingQualityScorecardQuery : IRequest<ManufacturingQualityScorecardModel>;
