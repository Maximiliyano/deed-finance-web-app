using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Capitals.Commands.PatchIncludeInTotal;

public sealed record PatchCapitalIncludeInTotalCommand(
    int Id,
    bool IncludeInTotal
) : ICommand;
