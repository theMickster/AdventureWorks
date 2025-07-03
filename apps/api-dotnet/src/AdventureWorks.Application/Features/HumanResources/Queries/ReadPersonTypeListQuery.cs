using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public class ReadPersonTypeListQuery : IRequest<List<PersonTypeModel>>
{
}
