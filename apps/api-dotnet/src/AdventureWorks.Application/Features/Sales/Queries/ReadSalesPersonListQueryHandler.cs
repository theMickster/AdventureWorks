using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class ReadSalesPersonListQueryHandler(
    IMapper mapper,
    ISalesPersonRepository salesPersonRepository)
        : IRequestHandler<ReadSalesPersonListQuery, SalesPersonSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesPersonRepository _salesPersonRepository = salesPersonRepository ?? throw new ArgumentNullException(nameof(salesPersonRepository));

    public async Task<SalesPersonSearchResultModel> Handle(ReadSalesPersonListQuery request, CancellationToken cancellationToken)
    {
        var result = new SalesPersonSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0
        };

        IReadOnlyList<SalesPersonEntity> salesPersonEntities;
        var totalRecords = 0;

        if (request.SearchModel is null)
        {
            (salesPersonEntities, totalRecords) = await _salesPersonRepository.GetSalesPersonsAsync(request.Parameters);
        }
        else
        {
            (salesPersonEntities, totalRecords) = await _salesPersonRepository.SearchSalesPersonsAsync(request.Parameters, request.SearchModel);
        }

        if (salesPersonEntities is null or { Count: 0 })
        {
            return result;
        }

        var salesPersons = _mapper.Map<List<SalesPersonModel>>(salesPersonEntities);

        result.Results = salesPersons;
        result.TotalRecords = totalRecords;

        return result;
    }
}
