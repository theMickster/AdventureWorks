using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadStoreSalesPersonAssignmentListQuery : IRequest<List<StoreSalesPersonAssignmentModel>>
{
    public required int StoreId { get; set; }
}
