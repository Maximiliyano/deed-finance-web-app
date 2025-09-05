using Deed.Domain.Enums;

namespace Deed.Application.Categories.Response;

public sealed record CategoryResponse(
    int Id,
    string Name,
    CategoryType Type,
    string PeriodType,
    float PeriodAmount);
