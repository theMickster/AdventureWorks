using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSpecialOfferListQuery : IRequest<List<SpecialOfferModel>>
{
}
