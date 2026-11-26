using Deed.Application.Capitals.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankNotifications.Import;

internal sealed class BankTransactionImporter(
    ICapitalRepository capitalRepository,
    IExpenseRepository expenseRepository,
    IIncomeRepository incomeRepository)
    : IBankTransactionImporter
{
    private const int MaxPurposeLength = 255;

    public async Task<Result> ImportAsync(
        BankConnection connection,
        BankNotification notification,
        int? categoryId,
        CancellationToken cancellationToken)
    {
        if (notification.Kind != BankNotificationKind.Transaction
            || notification.Direction == BankTransactionDirection.None
            || notification.ImportStatus == BankImportStatus.Imported)
        {
            return Result.Failure(DomainErrors.Bank.NotPending);
        }

        if (connection.CapitalId is not { } capitalId)
        {
            return Result.Failure(DomainErrors.Bank.CapitalNotLinked);
        }

        var capital = await capitalRepository
            .GetAsync(new CapitalByIdSpecification(capitalId, notification.CreatedBy, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(capital)));
        }

        if (!Enum.TryParse<CurrencyType>(notification.Currency, true, out var currency) || currency != capital.Currency)
        {
            return Result.Failure(DomainErrors.Bank.CurrencyMismatch);
        }

        var outgoing = notification.Direction == BankTransactionDirection.Outgoing;

        if (outgoing && capital.OnlyForSavings)
        {
            return Result.Failure(DomainErrors.Capital.ForSavingsOnly);
        }

        var resolvedCategoryId = categoryId ?? (outgoing ? connection.ExpenseCategoryId : connection.IncomeCategoryId);

        if (resolvedCategoryId is null)
        {
            return Result.Failure(DomainErrors.Bank.CategoryRequired);
        }

        var purpose = BuildPurpose(notification);

        if (outgoing)
        {
            expenseRepository.Create(new Expense
            {
                Amount = notification.Amount,
                PaymentDate = notification.OccurredAt,
                CategoryId = resolvedCategoryId.Value,
                CapitalId = capital.Id,
                Purpose = purpose,
                CreatedBy = notification.CreatedBy
            });

            capital.Balance -= notification.Amount;
        }
        else
        {
            incomeRepository.Create(new Income
            {
                Amount = notification.Amount,
                PaymentDate = notification.OccurredAt,
                CategoryId = resolvedCategoryId.Value,
                CapitalId = capital.Id,
                Purpose = purpose,
                CreatedBy = notification.CreatedBy
            });

            capital.Balance += notification.Amount;
        }

        notification.ImportStatus = BankImportStatus.Imported;

        return Result.Success();
    }

    private static string? BuildPurpose(BankNotification notification)
    {
        var parts = new[] { notification.Counterparty, notification.Purpose }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim());

        var text = string.Join(" - ", parts);

        if (text.Length == 0)
        {
            return null;
        }

        return text.Length <= MaxPurposeLength ? text : text[..MaxPurposeLength];
    }
}
