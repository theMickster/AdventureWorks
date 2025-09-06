using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadScrapReasonListQuery : IRequest<List<ScrapReasonModel>>
{
}
