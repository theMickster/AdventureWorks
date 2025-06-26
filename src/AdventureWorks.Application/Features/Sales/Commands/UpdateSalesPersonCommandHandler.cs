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
        var currentEntity = await _salesPersonRepository.GetByIdAsync(request.Model.Id);
        _mapper.Map(request.Model, currentEntity);
        currentEntity.ModifiedDate = request.ModifiedDate;
        await _salesPersonRepository.UpdateAsync(currentEntity);
    }
}
