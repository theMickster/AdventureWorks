using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for PatchEmployeeAddressCommand.
/// Applies JSON Patch operations to an employee's address.
/// </summary>
public sealed class PatchEmployeeAddressCommandHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository,
    IAddressRepository addressRepository,
    IValidator<EmployeeAddressUpdateModel> validator)
        : IRequestHandler<PatchEmployeeAddressCommand, Unit>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IAddressRepository _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
    private readonly IValidator<EmployeeAddressUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Unit> Handle(PatchEmployeeAddressCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.PatchDocument);

        // Verify employee exists
        var employee = await _employeeRepository.GetEmployeeByIdAsync(request.BusinessEntityId, cancellationToken);
        if (employee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {request.BusinessEntityId} not found.");
        }

        // Verify the address belongs to this employee
        var businessEntityAddress = await _employeeRepository.GetEmployeeAddressByIdAsync(
            request.BusinessEntityId,
            request.AddressId,
            cancellationToken);

        if (businessEntityAddress == null)
        {
            throw new KeyNotFoundException(
                $"Address with ID {request.AddressId} not found for employee {request.BusinessEntityId}.");
        }

        // Get the address entity with tracking enabled
        var addressEntity = await _addressRepository.GetByIdAsync(request.AddressId);

        if (addressEntity == null)
        {
            throw new KeyNotFoundException($"Address with ID {request.AddressId} not found.");
        }

        // Create a model from the current address entity
        var addressUpdateModel = new EmployeeAddressUpdateModel
        {
            AddressId = addressEntity.AddressId,
            AddressLine1 = addressEntity.AddressLine1,
            AddressLine2 = addressEntity.AddressLine2,
            City = addressEntity.City,
            StateProvinceId = addressEntity.StateProvinceId,
            PostalCode = addressEntity.PostalCode
        };

        // Apply the patch operations
        request.PatchDocument.ApplyTo(addressUpdateModel);

        // Validate the patched model
        await _validator.ValidateAndThrowAsync(addressUpdateModel, cancellationToken);

        // Apply changes to the entity
        addressEntity.AddressLine1 = addressUpdateModel.AddressLine1;
        addressEntity.AddressLine2 = addressUpdateModel.AddressLine2;
        addressEntity.City = addressUpdateModel.City;
        addressEntity.StateProvinceId = addressUpdateModel.StateProvinceId;
        addressEntity.PostalCode = addressUpdateModel.PostalCode;
        addressEntity.ModifiedDate = request.ModifiedDate;

        await _addressRepository.UpdateAsync(addressEntity);

        return Unit.Value;
    }
}
