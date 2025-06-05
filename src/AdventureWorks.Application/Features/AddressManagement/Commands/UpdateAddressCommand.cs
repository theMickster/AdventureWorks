using AdventureWorks.Models.Features.AddressManagement;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Commands;

public sealed class UpdateAddressCommand : IRequest
{
    public required AddressUpdateModel Model { get; set; }

    public DateTime ModifiedDate { get; set; }
}