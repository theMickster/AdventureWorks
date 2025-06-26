using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class UpdateSalesPersonCommand : IRequest
{
    public required SalesPersonUpdateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }
}
