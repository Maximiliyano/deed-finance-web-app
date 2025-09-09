using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.Capitals.Commands.Update;

public sealed record UpdateCapitalCommand(
    int Id,
    string? Name = null,
    float? Balance = null,
    CurrencyType? Currency = null,
    bool? IncludeInTotal = null,
    bool? OnlyForSavings = null
) : ICommand;
