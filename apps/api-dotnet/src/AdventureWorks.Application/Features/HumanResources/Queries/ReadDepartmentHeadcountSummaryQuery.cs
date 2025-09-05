using AdventureWorks.Models.Features.HumanResources;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadDepartmentHeadcountSummaryQuery : IRequest<IReadOnlyList<DepartmentHeadcountSummaryModel>>;
