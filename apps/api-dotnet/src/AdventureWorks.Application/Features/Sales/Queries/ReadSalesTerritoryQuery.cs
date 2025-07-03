using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesTerritoryQuery : IRequest<SalesTerritoryModel>
{
    public required int Id { get; set; } = int.MinValue;
}
