using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Tags.Commands.Delete;

public sealed record DeleteTagCommand(
    int Id
) : ICommand;
