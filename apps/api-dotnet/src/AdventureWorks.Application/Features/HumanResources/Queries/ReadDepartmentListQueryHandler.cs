using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadDepartmentListQueryHandler(
    IMapper mapper,
    IDepartmentRepository departmentRepository)
    : IRequestHandler<ReadDepartmentListQuery, List<DepartmentModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDepartmentRepository _repository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    public async Task<List<DepartmentModel>> Handle(ReadDepartmentListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _repository.ListAllAsync();
        return entities is not { Count: > 0 } ? [] : _mapper.Map<List<DepartmentModel>>(entities);
    }
}
