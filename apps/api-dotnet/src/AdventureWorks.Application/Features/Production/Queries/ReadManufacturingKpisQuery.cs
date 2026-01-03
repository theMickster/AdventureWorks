using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query that returns aggregate manufacturing KPI values across all production work orders.
/// </summary>
public sealed class ReadManufacturingKpisQuery : IRequest<ManufacturingKpisModel>;
