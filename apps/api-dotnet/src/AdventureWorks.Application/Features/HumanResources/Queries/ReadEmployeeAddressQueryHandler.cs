using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Queries;

/// <summary>
/// Handler for retrieving a specific address for an employee.
/// </summary>
public sealed class ReadEmployeeAddressQueryHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository)
        : IRequestHandler<ReadEmployeeAddressQuery, EmployeeAddressModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _repository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

    public async Task<EmployeeAddressModel?> Handle(ReadEmployeeAddressQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var businessEntityAddress = await _repository.GetEmployeeAddressByIdAsync(
            request.BusinessEntityId,
            request.AddressId,
            cancellationToken);

        if (businessEntityAddress == null)
        {
            return null;
        }

        return _mapper.Map<EmployeeAddressModel>(businessEntityAddress);
    }
}
