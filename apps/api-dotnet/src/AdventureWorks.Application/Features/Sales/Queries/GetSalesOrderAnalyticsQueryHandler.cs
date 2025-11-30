using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

public sealed class GetSalesOrderAnalyticsQueryHandler(
    ISalesOrderRepository salesOrderRepository,
    IValidator<GetSalesOrderAnalyticsQuery> validator)
    : IRequestHandler<GetSalesOrderAnalyticsQuery, SalesOrderAnalyticsModel>
{
    private readonly ISalesOrderRepository _salesOrderRepository =
        salesOrderRepository ?? throw new ArgumentNullException(nameof(salesOrderRepository));
    private readonly IValidator<GetSalesOrderAnalyticsQuery> _validator =
        validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<SalesOrderAnalyticsModel> Handle(
        GetSalesOrderAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        return await _salesOrderRepository.GetSalesOrderAnalyticsAsync(request.Filter, cancellationToken);
    }
}
