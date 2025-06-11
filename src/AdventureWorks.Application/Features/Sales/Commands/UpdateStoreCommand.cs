using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class UpdateStoreCommand : IRequest
{
    public required StoreUpdateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }
}
