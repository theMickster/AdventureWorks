using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadStoreListQuery : IRequest<StoreSearchResultModel>
{
    public required StoreParameter Parameters { get; set; }

    public StoreSearchModel? SearchModel { get; set; } 
}
