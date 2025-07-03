using AdventureWorks.Models.Features.AddressManagement;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Queries;

public sealed class ReadStateProvinceQuery : IRequest<StateProvinceModel>
{
    public required int Id { get; set; } = int.MinValue;
}
