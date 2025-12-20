using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Purchasing.Queries;

/// <summary>
/// Handler for retrieving the paginated, risk-ranked vendor list.
/// </summary>
/// <remarks>
/// No <c>IMapper</c> dependency, unlike most query handlers in this codebase — <see cref="IVendorRepository.GetVendorsAsync"/>
/// projects directly to <see cref="VendorModel"/> because <c>TotalSpend</c>, <c>PoCount</c>, and
/// <c>IsHighRisk</c> are query-computed values with no entity-to-model field mapping to perform.
/// </remarks>
public sealed class ReadVendorListQueryHandler(
    IVendorRepository vendorRepository,
    IValidator<ReadVendorListQuery> validator)
    : IRequestHandler<ReadVendorListQuery, VendorSearchResultModel>
{
    private readonly IVendorRepository _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
    private readonly IValidator<ReadVendorListQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Handles the query to retrieve a paginated, risk-ranked list of vendors.
    /// </summary>
    /// <param name="request">the query request containing pagination and filter parameters</param>
    /// <param name="cancellationToken">token to cancel the operation</param>
    /// <returns>A search result model containing the paginated vendor list</returns>
    public async Task<VendorSearchResultModel> Handle(
        ReadVendorListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var result = new VendorSearchResultModel
        {
            PageNumber = request.Parameters.PageNumber,
            PageSize = request.Parameters.PageSize,
            TotalRecords = 0,
            Results = new List<VendorModel>()
        };

        var (vendors, totalRecords) = await _vendorRepository.GetVendorsAsync(request.Parameters, cancellationToken);

        result.TotalRecords = totalRecords;

        if (vendors is null or { Count: 0 })
        {
            return result;
        }

        result.Results = vendors.ToList();

        return result;
    }
}
