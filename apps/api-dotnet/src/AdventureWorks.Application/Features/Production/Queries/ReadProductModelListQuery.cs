using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductModelListQuery : IRequest<List<ProductModelListModel>>
{
}
