using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Exchanges;
using Deed.Application.UtilityBills.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.UtilityBills.Commands.Pay;

internal sealed class PayUtilityBillCommandHandler(
    IUtilityBillRepository billRepository,
    ICapitalRepository capitalRepository,
    IExpenseRepository expenseRepository,
    IExchangeRateProvider exchangeRateProvider,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<PayUtilityBillCommand, int>
{
    public async Task<Result<int>> Handle(PayUtilityBillCommand command, CancellationToken cancellationToken)
    {
        var bill = await billRepository
            .GetAsync(new UtilityBillByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (bill is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound("utility bill"));
        }

        var capital = await capitalRepository
            .GetAsync(new CapitalByIdSpecification(bill.CapitalId, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound(nameof(capital)));
        }

        if (capital.OnlyForSavings)
        {
            return Result.Failure<int>(DomainErrors.Capital.ForSavingsOnly);
        }

        var paymentDate = command.PaymentDate ?? DateTimeOffset.UtcNow;
        var billAmount = command.Amount;
        decimal? meterReading = null;

        if (bill.UnitPrice is { } unitPrice)
        {
            if (command.MeterReading is not { } currentReading)
            {
                return Result.Failure<int>(DomainErrors.UtilityBill.MeterReadingRequired);
            }

            var (from, to) = UtilityBillMonths.Previous(paymentDate);

            var previousReadings = await expenseRepository
                .GetLatestMeterReadingsAsync([bill.Id], from, to, cancellationToken)
                .ConfigureAwait(false);

            var previousReading = previousReadings.GetValueOrDefault(bill.Id);

            if (currentReading <= previousReading)
            {
                return Result.Failure<int>(DomainErrors.UtilityBill.MeterReadingNotIncreasing);
            }

            billAmount = Math.Round((currentReading - previousReading) * unitPrice, 2, MidpointRounding.AwayFromZero);
            meterReading = currentReading;
        }

        var amount = await exchangeRateProvider
            .ConvertAsync(billAmount, bill.Currency, capital.Currency, cancellationToken)
            .ConfigureAwait(false);

        if (!amount.IsSuccess)
        {
            return Result.Failure<int>(amount.Errors);
        }

        var expense = new Expense
        {
            Amount = amount.Value,
            CategoryId = bill.CategoryId,
            CapitalId = bill.CapitalId,
            PaymentDate = paymentDate,
            Purpose = bill.Name,
            MeterReading = meterReading,
            UtilityBillId = bill.Id
        };

        capital.Balance -= expense.Amount;

        capitalRepository.Update(capital);
        expenseRepository.Create(expense);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("UtilityBill {Id} paid with Expense {ExpenseId}", bill.Id, expense.Id);

        return Result.Success(expense.Id);
    }
}
