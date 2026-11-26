using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankNotifications.Commands.Dismiss;

public sealed record DismissBankNotificationCommand(int Id) : ICommand;
