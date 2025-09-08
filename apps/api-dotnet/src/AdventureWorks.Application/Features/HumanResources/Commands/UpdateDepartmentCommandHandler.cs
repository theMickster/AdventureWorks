using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

public sealed class UpdateDepartmentCommandHandler(
    IMapper mapper,
    IDepartmentRepository departmentRepository,
    IValidator<DepartmentUpdateModel> validator)
        : IRequestHandler<UpdateDepartmentCommand, Unit>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDepartmentRepository _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly IValidator<DepartmentUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Unit> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var existing = await _departmentRepository.GetByIdAsync(request.Model.Id, cancellationToken);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Department with ID {request.Model.Id} not found.");
        }

        _mapper.Map(request.Model, existing);
        existing.ModifiedDate = request.ModifiedDate;

        await _departmentRepository.UpdateAsync(existing, cancellationToken);

        return Unit.Value;
    }
}
