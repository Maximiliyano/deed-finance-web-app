using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankNotifications.Commands.MarkRead;

public sealed record MarkBankNotificationsReadCommand(IReadOnlyList<int>? Ids) : ICommand;
