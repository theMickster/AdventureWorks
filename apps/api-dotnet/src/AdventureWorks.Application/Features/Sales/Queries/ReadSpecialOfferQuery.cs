using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSpecialOfferQuery : IRequest<SpecialOfferModel?>
{
    public required int Id { get; set; }
}
