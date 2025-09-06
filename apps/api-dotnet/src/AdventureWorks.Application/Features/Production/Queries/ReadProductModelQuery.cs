using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

public sealed class ReadProductModelQuery : IRequest<ProductModelDetailModel?>
{
    public required int Id { get; set; } = int.MinValue;
}
