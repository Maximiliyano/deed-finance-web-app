using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.Categories.Commands.Update;

public sealed record UpdateCategoryCommand(
    int Id,
    string? Name = null,
    decimal? PlannedPeriodAmount = null,
    PerPeriodType? Period = null,
    CategoryType? Type = null)
    : ICommand;
