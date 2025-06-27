﻿using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

internal class ReadSalesPersonQueryHandler(
    IMapper mapper,
    ISalesPersonRepository salesPersonRepository)
        : IRequestHandler<ReadSalesPersonQuery, SalesPersonModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesPersonRepository _repository = salesPersonRepository ?? throw new ArgumentNullException(nameof(salesPersonRepository));

    public async Task<SalesPersonModel> Handle(ReadSalesPersonQuery request, CancellationToken cancellationToken)
    {
        return _mapper.Map<SalesPersonModel>(await _repository.GetByIdAsync(request.Id));
    }
}
