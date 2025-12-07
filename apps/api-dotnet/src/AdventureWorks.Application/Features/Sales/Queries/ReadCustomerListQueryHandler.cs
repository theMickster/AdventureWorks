using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Queries;

/// <summary>
/// Handler for <see cref="ReadCustomerListQuery"/>. Loads the requested page of
/// LTV-ranked customer projections via a narrow EF projection and maps them to the read model.
/// </summary>
public sealed class ReadCustomerListQueryHandler(
    IMapper mapper,
    ICustomerRepository customerRepository,
    IValidator<ReadCustomerListQuery> validator)
    : IRequestHandler<ReadCustomerListQuery, CustomerSearchResultModel>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ICustomerRepository _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    private readonly IValidator<ReadCustomerListQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<CustomerSearchResultModel> Handle(ReadCustomerListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var (projections, totalCount) = await _customerRepository.GetCustomersAsync(request.Parameters, cancellationToken);

        return new CustomerSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = totalCount,
            Results = _mapper.Map<List<CustomerListItemModel>>(projections)
        };
    }
}
