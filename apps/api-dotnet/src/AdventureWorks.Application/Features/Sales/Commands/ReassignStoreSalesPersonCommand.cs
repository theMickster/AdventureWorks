using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class ReassignStoreSalesPersonCommand : IRequest<int>
{
    public int StoreId { get; set; }

    public required StoreSalesPersonAssignmentCreateModel Model { get; set; }

    public DateTime AssignedDate { get; set; }
}
