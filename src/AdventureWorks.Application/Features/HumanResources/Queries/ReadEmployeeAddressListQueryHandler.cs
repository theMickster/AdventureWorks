using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Handler for retrieving all addresses for an employee.
/// </summary>
public sealed class ReadEmployeeAddressListQueryHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository)
        : IRequestHandler<ReadEmployeeAddressListQuery, IReadOnlyList<EmployeeAddressModel>>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _repository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

    public async Task<IReadOnlyList<EmployeeAddressModel>> Handle(ReadEmployeeAddressListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var businessEntityAddresses = await _repository.GetEmployeeAddressesAsync(
            request.BusinessEntityId,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<EmployeeAddressModel>>(businessEntityAddresses);
    }
}
