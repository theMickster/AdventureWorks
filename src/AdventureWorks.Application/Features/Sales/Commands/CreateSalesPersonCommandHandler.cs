using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handler for CreateSalesPersonCommand.
/// Builds complete entity graph (BusinessEntity → Person → Employee → SalesPerson)
/// and delegates to repository for cascade inserts.
/// </summary>
public sealed class CreateSalesPersonCommandHandler(
    IMapper mapper,
    ISalesPersonRepository salesPersonRepository,
    IValidator<SalesPersonCreateModel> validator)
        : IRequestHandler<CreateSalesPersonCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesPersonRepository _salesPersonRepository = salesPersonRepository ?? throw new ArgumentNullException(nameof(salesPersonRepository));
    private readonly IValidator<SalesPersonCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(CreateSalesPersonCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        // Build SalesPerson entity
        var salesPersonEntity = _mapper.Map<SalesPersonEntity>(request.Model);

        // Build Employee entity
        var employeeEntity = new EmployeeEntity
        {
            NationalIdnumber = request.Model.NationalIdNumber,
            LoginId = request.Model.LoginId,
            JobTitle = request.Model.JobTitle,
            BirthDate = request.Model.BirthDate,
            HireDate = request.Model.HireDate,
            MaritalStatus = request.Model.MaritalStatus,
            Gender = request.Model.Gender,
            SalariedFlag = request.Model.SalariedFlag,
            OrganizationLevel = request.Model.OrganizationLevel
        };

        // Build Person entity
        var personEntity = new PersonEntity
        {
            FirstName = request.Model.FirstName,
            LastName = request.Model.LastName,
            MiddleName = request.Model.MiddleName,
            Title = request.Model.Title,
            Suffix = request.Model.Suffix
        };

        // Build PersonPhone
        var personPhone = new PersonPhone
        {
            PhoneNumber = request.Model.Phone.PhoneNumber,
            PhoneNumberTypeId = request.Model.Phone.PhoneNumberTypeId
        };

        // Build EmailAddress
        var emailAddress = new EmailAddressEntity
        {
            EmailAddressName = request.Model.EmailAddress
        };

        // Build Address
        var address = _mapper.Map<AddressEntity>(request.Model.Address);

        var businessEntityId = await _salesPersonRepository.CreateSalesPersonWithEmployeeAsync(
            salesPersonEntity,
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
