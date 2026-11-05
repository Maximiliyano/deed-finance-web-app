using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Domain.Enums;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.UserSettings.Commands.Upsert;

internal sealed class UpsertUserSettingsCommandHandler(
    IUserSettingsRepository repository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpsertUserSettingsCommand>
{
    public async Task<Result> Handle(UpsertUserSettingsCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.GetAsync(user.Name!, cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            var entity = command.ToEntity();
            entity.Email = user.Email;
            repository.Create(entity);
        }
        else
        {
            var updatedCurrency = Enum.Parse<CurrencyType>(command.Currency);
            if (existing.Currency == updatedCurrency &&
                existing.BalanceReminderEnabled == command.BalanceReminderEnabled &&
                existing.BalanceReminderCron == command.BalanceReminderCron &&
                existing.ExpenseReminderEnabled == command.ExpenseReminderEnabled &&
                existing.ExpenseReminderCron == command.ExpenseReminderCron &&
                existing.DebtReminderEnabled == command.DebtReminderEnabled &&
                existing.DebtReminderCron == command.DebtReminderCron &&
                existing.EmailNotificationsEnabled == command.EmailNotificationsEnabled &&
                existing.Email == user.Email)
            {
                return Result.Success();
            }

            existing.Currency = updatedCurrency;
            existing.BalanceReminderEnabled = command.BalanceReminderEnabled;
            existing.BalanceReminderCron = command.BalanceReminderCron;
            existing.ExpenseReminderEnabled = command.ExpenseReminderEnabled;
            existing.ExpenseReminderCron = command.ExpenseReminderCron;
            existing.DebtReminderEnabled = command.DebtReminderEnabled;
            existing.DebtReminderCron = command.DebtReminderCron;
            existing.EmailNotificationsEnabled = command.EmailNotificationsEnabled;
            existing.Email = user.Email;
            
            repository.Update(existing);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("UserSettings upserted for {User}", user.Name);

        return Result.Success();
    }
}
