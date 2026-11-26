using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Capitals.Commands.PatchOnlyForSavings;

public sealed record PatchCapitalSetForSavingsCommand(
    int Id, 
    bool OnlyForSavings
) : ICommand;
