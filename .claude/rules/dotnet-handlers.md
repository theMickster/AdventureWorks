---
paths:
  - "apps/api-dotnet/**/*.cs"
---

# .NET Handler Implementation Checklist

Ordered steps — verify each before moving to the next.

## Class Declaration

```csharp
public sealed class {Name}Handler(
    IMapper mapper,
    IRepository repository,
    IValidator<TModel> validator)  // validators on commands only
        : IRequestHandler<TRequest, TResponse>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
}
```

- `sealed` — always
- Primary constructor (C# 12) with field assignment and null checks
- No `[Inject]` attributes

## Handle Method — Command (Write)

Execute these steps in order:

1. `ArgumentNullException.ThrowIfNull(request);`
2. `ArgumentNullException.ThrowIfNull(request.Model);`
3. `await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);`
4. Map to entity: `_mapper.Map<TEntity>(request.Model)`
5. Set audit fields (ModifiedDate, Rowguid)
6. Persist via repository
7. Return the new ID

## Handle Method — Query (Read)

1. `ArgumentNullException.ThrowIfNull(request);`
2. Fetch via repository (all reads use `.AsNoTracking()` in the repo)
3. Map to DTO: `_mapper.Map<TModel>(entity)`
4. Return DTO (never return an entity)

## Async Rules

- `CancellationToken` parameter on all async methods — pass it through to every async call
- Never use `.Result`, `.Wait()`, or `Task.Run()` for I/O

## Data Access

- Inject repository interfaces only — never `DbContext`
- Repository interfaces live in `Application/PersistenceContracts/`
- Implementations live in `Infrastructure.Persistence/Repositories/`
