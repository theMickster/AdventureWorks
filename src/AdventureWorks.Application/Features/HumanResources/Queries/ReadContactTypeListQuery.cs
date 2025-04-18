using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public class ReadContactTypeListQuery : IRequest<List<ContactTypeModel>>
{
}
