using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public class ReadContactTypeQuery : IRequest<ContactTypeModel>
{
    public required int Id { get; set; } = int.MinValue;
}