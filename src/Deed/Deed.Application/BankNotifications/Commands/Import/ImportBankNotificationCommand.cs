using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.BankNotifications.Commands.Import;

public sealed record ImportBankNotificationCommand(int Id, int? CategoryId) : ICommand;
