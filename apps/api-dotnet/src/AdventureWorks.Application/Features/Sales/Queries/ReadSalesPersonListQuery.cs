using AdventureWorks.Common.Filtering;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesPersonListQuery : IRequest<SalesPersonSearchResultModel>
{
    public required SalesPersonParameter Parameters { get; set; }

    public SalesPersonSearchModel? SearchModel { get; set; }
}
