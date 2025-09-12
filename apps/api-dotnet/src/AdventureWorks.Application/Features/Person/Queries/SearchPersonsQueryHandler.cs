using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Models.Features.Person;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Person.Queries;

/// <summary>
/// Handler for <see cref="SearchPersonsQuery"/>.
/// </summary>
public sealed class SearchPersonsQueryHandler(
    IMapper mapper,
    IPersonRepository personRepository,
    IValidator<SearchPersonsQuery> validator)
        : IRequestHandler<SearchPersonsQuery, SearchPersonsQueryResult>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IPersonRepository _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    private readonly IValidator<SearchPersonsQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    /// <summary>
    /// Handles the search persons query by validating input, searching the repository, and mapping results.
    /// </summary>
    /// <param name="request">The search query with optional filters and pagination parameters.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A result containing paginated persons matching the search criteria.</returns>
    public async Task<SearchPersonsQueryResult> Handle(SearchPersonsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var (persons, totalCount) = await _personRepository.SearchAsync(
            request.FirstName,
            request.LastName,
            request.PersonTypeCode,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = persons.Select(p => _mapper.Map<SearchPersonsModel>(p)).ToList();

        return new SearchPersonsQueryResult
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };
    }
}
