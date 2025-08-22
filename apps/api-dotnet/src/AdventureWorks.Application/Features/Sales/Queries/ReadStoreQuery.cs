using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadStoreQuery : IRequest<StoreModel>
{
    public required int Id { get; set; } = int.MinValue;

    public bool IncludeAddresses { get; init; } = true;

    public bool IncludeContacts { get; init; } = true;
}
