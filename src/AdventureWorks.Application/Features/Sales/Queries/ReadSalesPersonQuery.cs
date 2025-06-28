using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesPersonQuery : IRequest<SalesPersonModel?>
{
    public required int Id { get; set; } = int.MinValue;
}