---
paths:
  - "apps/api-dotnet/src/AdventureWorks.Infrastructure.Persistence/**/*.cs"
  - "apps/api-dotnet/src/AdventureWorks.Common/Filtering/**/*.cs"
---

# EF Core Repository Query Rules

Invariants that no test or code comment enforces. Each cites the repository that shows the shape.

- Keep aggregate `GroupBy` queries join-free and join in memory afterwards. An untranslatable `GroupBy` throws only on SQL Server, and the InMemory provider used by every repository test evaluates it happily, so a test cannot catch the regression. See `VendorRepository.GetVendorsAsync` (two separate queries joined in C#) and `SalesOrderRepository.GetSalesOrderAnalyticsAsync` (bare `AsNoTracking()` base, no `Include`).
- Add any new `SearchSalesOrdersAsync` filter to `GetSalesOrderAnalyticsAsync` as well. The nullable filter predicates are duplicated by hand in `SalesOrderRepository.cs`, and only the customer-id filter is tested in both places.
- Expose a new entity by appending `DbSet<T>` to both `AdventureWorksDbContext` and `IAdventureWorksDbContext`, without reordering existing declarations. Repositories use the concrete context, so the compiler catches a missing concrete `DbSet` but never a missing interface one.
- Change a list's default page size by overriding `PageSize` with `override` and its own backing field, never a `new` hide. `QueryStringParamsBase.GetRecordsToSkip()` reads `PageSize` through the base, so only a true override is honored. See `VendorParameter` and `WorkOrderParameter`.
- Read-only repository methods call `.AsNoTracking()`. The generic `ReadOnlyEfRepository` base does not, so override it for list reads.
