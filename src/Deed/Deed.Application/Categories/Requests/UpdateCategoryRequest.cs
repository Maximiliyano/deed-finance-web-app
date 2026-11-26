using Deed.Domain.Enums;

namespace Deed.Application.Categories.Requests;

public sealed record UpdateCategoryRequest(
    int Id,
    string? Name,
    CategoryType? Type,
    decimal? PeriodAmount,
    TimePeriodType? PeriodType
);
