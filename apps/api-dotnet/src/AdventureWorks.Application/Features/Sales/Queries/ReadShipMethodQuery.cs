using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadShipMethodQuery : IRequest<ShipMethodModel?>
{
    public required int Id { get; set; } = int.MinValue;
}
