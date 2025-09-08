using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.HumanResources.Commands;

public sealed class CreateDepartmentCommandHandler(
    IMapper mapper,
    IDepartmentRepository departmentRepository,
    IValidator<DepartmentCreateModel> validator)
        : IRequestHandler<CreateDepartmentCommand, short>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDepartmentRepository _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    private readonly IValidator<DepartmentCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<short> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var entity = _mapper.Map<DepartmentEntity>(request.Model);
        entity.ModifiedDate = request.ModifiedDate;

        var created = await _departmentRepository.AddAsync(entity, cancellationToken);

        return created.DepartmentId;
    }
}
