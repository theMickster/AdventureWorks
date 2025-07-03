using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public class ReadPersonTypeQuery : IRequest<PersonTypeModel>
{
    public required int Id { get; set; } = int.MinValue;
}
