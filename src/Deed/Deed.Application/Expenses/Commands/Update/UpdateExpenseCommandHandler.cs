using Deed.Application.Abstractions.Messaging;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Expenses.Commands.Update;

internal sealed class UpdateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateExpenseCommand>
{
    public async Task<Result> Handle(UpdateExpenseCommand command, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetAsync(new ExpenseByIdSpecification(command.Id, true, true, true, enableTracking: true)).ConfigureAwait(false);

        if (expense?.Capital is null || expense?.Category is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(expense)));
        }

        if (expense.Capital.OnlyForSavings)
        {
            return Result.Failure(DomainErrors.Capital.ForSavingsOnly);
        }

        if (HasNoChanges(command, expense))
        {
            return Result.Success();
        }

        var changed = false;

        if (command.Amount is not null && command.Amount != expense.Amount)
        {
            var difference = expense.Amount - command.Amount.Value;

            expense.Capital.Balance += difference;

            expense.Amount = command.Amount.Value;
            changed = true;
        }

        if (command.CapitalId.HasValue && command.CapitalId.Value != expense.CapitalId)
        {
            expense.CapitalId = command.CapitalId.Value;
            changed = true;
        }

        if (command.TagNames is not null && command.TagNames.Any())
        {
            expense.Tags.Clear();
            expense.Tags.AddRange(command.TagNames.Select(t => new ExpenseTag
            {
                Expense = expense,
                Tag = new Tag { Name = t },
            }));
            changed = true;
        }

        if (command.Purpose is not null && command.Purpose != expense.Purpose)
        {
            expense.Purpose = command.Purpose;
            changed = true;
        }

        if (command.Date is not null && command.Date != expense.PaymentDate)
        {
            expense.PaymentDate = command.Date.Value;
            changed = true;
        }

        if (command.CategoryId.HasValue && command.CategoryId.Value != expense.CategoryId)
        {
            expense.CategoryId = command.CategoryId.Value;

            changed = true;
        }

        if (!changed)
        {
            return Result.Success();
        }

        expenseRepository.Update(expense);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private bool HasNoChanges(UpdateExpenseCommand command, Domain.Entities.Expense expense) =>
        (command.CategoryId is null || command.CategoryId == expense.CategoryId) &&
        (command.CapitalId is null || command.CapitalId == expense.CapitalId) &&
        (command.Amount is null || command.Amount == expense.Amount) &&
        (command.Date is null || command.Date == expense.PaymentDate) &&
        (command.Purpose is null || command.Purpose == expense.Purpose) &&
        (command.TagNames is null || command.TagNames.All(name => expense.Tags.Exists(t => t.Tag.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
    );
}
