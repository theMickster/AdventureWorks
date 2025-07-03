using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class CreateStoreCommand : IRequest<int>
{
    public required StoreCreateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }

    public Guid RowGuid { get; set; }
}
