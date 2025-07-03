using AdventureWorks.Models.Features.AddressManagement;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadAddressTypeQuery : IRequest<AddressTypeModel>
{
    public required int Id { get; set; } = int.MinValue;
}
