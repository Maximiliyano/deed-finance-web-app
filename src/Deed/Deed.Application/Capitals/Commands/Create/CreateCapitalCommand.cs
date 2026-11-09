using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Capitals.Commands.Create;

public sealed record CreateCapitalCommand(
    string Name,
    decimal Balance,
    string Currency,
    bool IncludeInTotal,
    bool OnlyForSavings)
    : ICommand<int>;
