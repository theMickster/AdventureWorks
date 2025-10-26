using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handles <see cref="ReadSalesPersonPerformanceQuery"/>; loads the entity with quota and territory history,
/// then applies order aggregates before mapping to the response model.
/// </summary>
public sealed class ReadSalesPersonPerformanceQueryHandler(
    IMapper mapper,
    ISalesPersonRepository salesPersonRepository)
        : IRequestHandler<ReadSalesPersonPerformanceQuery, SalesPersonPerformanceModel?>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ISalesPersonRepository _repository = salesPersonRepository ?? throw new ArgumentNullException(nameof(salesPersonRepository));

    /// <summary>
    /// Fetches the sales person with performance data and maps it to a <see cref="SalesPersonPerformanceModel"/>.
    /// </summary>
    /// <param name="request">the query containing the sales person id</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>the performance model, or <see langword="null"/> if the sales person does not exist</returns>
    public async Task<SalesPersonPerformanceModel?> Handle(ReadSalesPersonPerformanceQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await _repository.GetSalesPersonWithPerformanceDataAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var (orderCount, totalRevenue) = await _repository.GetSalesPersonOrderAggregatesAsync(request.Id, cancellationToken);

        var model = _mapper.Map<SalesPersonPerformanceModel>(entity);
        model.OrderCount = orderCount;
        model.TotalRevenue = totalRevenue;

        return model;
    }
}
