using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Enums;

namespace Deed.Application.Capitals.Commands.Create;

public sealed record CreateCapitalCommand(
    string Name,
    float Balance,
    CurrencyType Currency,
    bool IncludeInTotal,
    bool OnlyForSavings)
    : ICommand<int>;
