using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

/// <summary>
/// Handler for CreateEmployeeCommand.
/// Builds entity graph and delegates to repository for cascade inserts.
/// </summary>
public sealed class CreateEmployeeCommandHandler(
    IMapper mapper,
    IEmployeeRepository employeeRepository,
    IValidator<EmployeeCreateModel> validator)
        : IRequestHandler<CreateEmployeeCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IEmployeeRepository _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    private readonly IValidator<EmployeeCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var employeeEntity = _mapper.Map<EmployeeEntity>(request.Model);

        var personEntity = new PersonEntity
        {
            FirstName = request.Model.FirstName,
            LastName = request.Model.LastName,
            MiddleName = request.Model.MiddleName,
            Title = request.Model.Title,
            Suffix = request.Model.Suffix
        };

        var personPhone = new PersonPhone
        {
            PhoneNumber = request.Model.Phone.PhoneNumber,
            PhoneNumberTypeId = request.Model.Phone.PhoneNumberTypeId
        };

        var emailAddress = new EmailAddressEntity
        {
            EmailAddressName = request.Model.EmailAddress
        };

        var address = _mapper.Map<AddressEntity>(request.Model.Address);

        var businessEntityId = await _employeeRepository.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            personPhone,
            emailAddress,
            address,
            request.Model.AddressTypeId,
            request.ModifiedDate,
            request.RowGuid,
            cancellationToken);

        return businessEntityId;
    }
}
