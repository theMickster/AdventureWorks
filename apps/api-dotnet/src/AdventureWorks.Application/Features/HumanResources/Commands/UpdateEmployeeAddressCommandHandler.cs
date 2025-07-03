using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for UpdateEmployeeAddressCommand.
/// Updates the AddressEntity fields only (not BusinessEntityAddress).
/// </summary>
public sealed class UpdateEmployeeAddressCommandHandler(
    IEmployeeRepository employeeRepository,
    IAddressRepository addressRepository,
    IValidator<EmployeeAddressUpdateModel> validator)
        : IRequestHandler<UpdateEmployeeAddressCommand, Unit>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IAddressRepository _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
    private readonly IValidator<EmployeeAddressUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Unit> Handle(UpdateEmployeeAddressCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        // Validate the model
        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        // Verify employee exists
        var employee = await _employeeRepository.GetEmployeeByIdAsync(request.BusinessEntityId, cancellationToken);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.BusinessEntityId} not found.");
        }

        // Verify the address belongs to this employee
        var businessEntityAddress = await _employeeRepository.GetEmployeeAddressByIdAsync(
            request.BusinessEntityId,
            request.Model.AddressId,
            cancellationToken);

        if (businessEntityAddress == null)
        {
            throw new KeyNotFoundException(
                $"Address with ID {request.Model.AddressId} not found for employee {request.BusinessEntityId}.");
        }

        // Get the address entity with tracking enabled
        var addressEntity = await _addressRepository.GetByIdAsync(request.Model.AddressId);

        if (addressEntity == null)
        {
            throw new KeyNotFoundException($"Address with ID {request.Model.AddressId} not found.");
        }

        // Update address fields
        addressEntity.AddressLine1 = request.Model.AddressLine1;
        addressEntity.AddressLine2 = request.Model.AddressLine2;
        addressEntity.City = request.Model.City;
        addressEntity.StateProvinceId = request.Model.StateProvinceId;
        addressEntity.PostalCode = request.Model.PostalCode;
        addressEntity.ModifiedDate = request.ModifiedDate;

        await _addressRepository.UpdateAsync(addressEntity);

        return Unit.Value;
    }
}
