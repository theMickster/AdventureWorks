using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

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

        var inputEntity = _mapper.Map<SalesPersonEntity>(request.Model);
        inputEntity.ModifiedDate = request.ModifiedDate;
        inputEntity.Rowguid = request.RowGuid;

        var outputEntity = await _salesPersonRepository.AddAsync(inputEntity);

        return outputEntity.BusinessEntityId;
    }
}
