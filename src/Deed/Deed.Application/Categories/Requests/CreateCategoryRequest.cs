using Deed.Domain.Enums;

namespace Deed.Application.Categories.Requests;

public sealed record CreateCategoryRequest(
    string Name,
    CategoryType Type,
    decimal PlannedPeriodAmount,
    TimePeriodType Period);
