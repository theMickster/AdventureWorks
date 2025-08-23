using AdventureWorks.Models.Features.Production;
using MediatR;

namespace AdventureWorks.Application.Features.Production.Queries;

/// <summary>
/// Query to retrieve all product categories (4 categories in AdventureWorks).
/// </summary>
public sealed class ReadProductCategoriesQuery : IRequest<List<ProductCategoryModel>>
{
}
