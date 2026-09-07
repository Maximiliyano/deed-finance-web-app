using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
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
            .GetAsync(new CapitalByIdSpecification(bill.CapitalId), cancellationToken)
            .ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound(nameof(capital)));
        }

        if (capital.OnlyForSavings)
        {
            return Result.Failure<int>(DomainErrors.Capital.ForSavingsOnly);
        }

        var expense = new Expense
        {
            Amount = command.Amount,
            CategoryId = bill.CategoryId,
            CapitalId = bill.CapitalId,
            PaymentDate = command.PaymentDate ?? DateTimeOffset.UtcNow,
            Purpose = bill.Name,
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
