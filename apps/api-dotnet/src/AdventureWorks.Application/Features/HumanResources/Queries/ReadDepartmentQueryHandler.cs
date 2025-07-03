using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

public sealed class ReadDepartmentQueryHandler(
    IMapper mapper,
    IDepartmentRepository departmentRepository)
        : IRequestHandler<ReadDepartmentQuery, DepartmentModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDepartmentRepository _repository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    public async Task<DepartmentModel> Handle(ReadDepartmentQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<DepartmentModel>(await _repository.GetByIdAsync(request.Id));
    }
}
