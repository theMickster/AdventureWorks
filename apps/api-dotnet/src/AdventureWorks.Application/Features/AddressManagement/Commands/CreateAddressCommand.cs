using AdventureWorks.Models.Features.AddressManagement;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Commands;

public sealed class CreateAddressCommand : IRequest<int>
{
    public required AddressCreateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }

    public Guid RowGuid { get; set; } 
}
