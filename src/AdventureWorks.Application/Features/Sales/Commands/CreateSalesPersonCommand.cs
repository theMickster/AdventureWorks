using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class CreateSalesPersonCommand : IRequest<int>
{
    public required SalesPersonCreateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }

    public Guid RowGuid { get; set; }
}
