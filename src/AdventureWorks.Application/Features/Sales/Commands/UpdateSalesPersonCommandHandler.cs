using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

public sealed class UpdateSalesPersonCommandHandler(
    IMapper mapper,
    ISalesPersonRepository salesPersonRepository,
    IValidator<SalesPersonUpdateModel> validator)
        : IRequestHandler<UpdateSalesPersonCommand>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesPersonRepository _salesPersonRepository = salesPersonRepository ?? throw new ArgumentNullException(nameof(salesPersonRepository));
    private readonly IValidator<SalesPersonUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task Handle(UpdateSalesPersonCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        // Fetch existing entity WITH related data (Employee and Person)
        var currentEntity = await _salesPersonRepository.GetSalesPersonByIdAsync(request.Model.Id);

        // Map SalesPerson-specific fields via AutoMapper
        _mapper.Map(request.Model, currentEntity);

        // Manually update Employee fields (updatable only - immutable fields excluded)
        currentEntity.Employee.JobTitle = request.Model.JobTitle;
        currentEntity.Employee.MaritalStatus = request.Model.MaritalStatus;
        currentEntity.Employee.Gender = request.Model.Gender;
        currentEntity.Employee.SalariedFlag = request.Model.SalariedFlag;
        currentEntity.Employee.OrganizationLevel = request.Model.OrganizationLevel;

        // Manually update Person fields
        currentEntity.Employee.PersonBusinessEntity.FirstName = request.Model.FirstName;
        currentEntity.Employee.PersonBusinessEntity.LastName = request.Model.LastName;
        currentEntity.Employee.PersonBusinessEntity.MiddleName = request.Model.MiddleName;
        currentEntity.Employee.PersonBusinessEntity.Title = request.Model.Title;
        currentEntity.Employee.PersonBusinessEntity.Suffix = request.Model.Suffix;

        // Call cascade update with transaction
        await _salesPersonRepository.UpdateSalesPersonWithEmployeeAsync(
            currentEntity,
            currentEntity.Employee,
            currentEntity.Employee.PersonBusinessEntity,
            request.ModifiedDate,
            cancellationToken);
    }
}
