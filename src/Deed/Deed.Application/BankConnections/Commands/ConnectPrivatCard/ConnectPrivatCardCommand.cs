using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankConnections.Commands.ConnectPrivatCard;

public sealed record ConnectPrivatCardCommand(string MerchantId, string Password, string CardNumber) : ICommand;
