using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Response;
using Deed.Domain.Enums;

namespace Deed.Application.Categories.Queries.GetAll;

public sealed record GetAllCategoryQuery(
    CategoryType? Type = null,
    bool? IncludeDeleted = null)
    : IQuery<IEnumerable<CategoryResponse>>;
