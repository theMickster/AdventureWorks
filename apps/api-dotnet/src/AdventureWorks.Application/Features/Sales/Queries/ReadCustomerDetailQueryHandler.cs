using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for retrieving a single customer's full detail, including LTV rank and spend metrics.
/// </summary>
/// <remarks>
/// Throws <see cref="KeyNotFoundException"/> — caught by <c>ExceptionHandlerMiddleware</c> and translated
/// to a structured 404 body (<c>error</c>/<c>correlationId</c>/<c>timestamp</c>) — rather than returning
/// null for the controller to translate. This mirrors <see cref="ReadPurchaseOrderDetailQueryHandler"/>.
/// Builds <see cref="CustomerDetailModel"/> by hand rather than via AutoMapper because
/// <see cref="ICustomerRepository.GetCustomerDetailAsync"/> returns a persistence-layer
/// projection (<see cref="CustomerLtvProjection"/>), not the DTO itself — keeping
/// <c>AdventureWorks.Models</c> out of the Infrastructure layer while preserving the shared
/// ranking projection used by both the list and detail reads. This deviates from the vendor
/// and purchase order detail repositories, which return the DTO directly.
/// </remarks>
public sealed class ReadCustomerDetailQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<ReadCustomerDetailQuery, CustomerDetailModel>
{
    private readonly ICustomerRepository _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));

    /// <summary>
    /// Handles the query to retrieve a single customer's full detail.
    /// </summary>
    /// <param name="request">the query request containing the customer identifier</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>The customer detail</returns>
    /// <exception cref="KeyNotFoundException">no customer with <see cref="ReadCustomerDetailQuery.CustomerId"/> exists</exception>
    public async Task<CustomerDetailModel> Handle(ReadCustomerDetailQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (customer, totalCustomerCount) = await _customerRepository.GetCustomerDetailAsync(request.CustomerId, cancellationToken);

        if (customer is null)
        {
            throw new KeyNotFoundException($"Customer {request.CustomerId} was not found.");
        }

        return new CustomerDetailModel
        {
            CustomerId = customer.CustomerId,
            DisplayName = customer.DisplayName,
            LtvRank = customer.LtvRank,
            TotalCustomerCount = totalCustomerCount,
            TotalSpend = customer.TotalSpend,
            OrderCount = customer.OrderCount,
            AvgOrderValue = customer.OrderCount == 0 ? 0m : customer.TotalSpend / customer.OrderCount,
            LastOrderDate = customer.LastOrderDate,
            IsInactive = customer.IsInactive,
            CustomerType = customer.CustomerType,
            StoreId = customer.StoreId,
            StoreName = customer.StoreName,
            FirstName = customer.FirstName,
            LastName = customer.LastName
        };
    }
}
