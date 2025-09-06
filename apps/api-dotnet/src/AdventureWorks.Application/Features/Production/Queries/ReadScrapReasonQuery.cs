using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadScrapReasonQuery : IRequest<ScrapReasonModel?>
{
    public required int Id { get; set; } = int.MinValue;
}
