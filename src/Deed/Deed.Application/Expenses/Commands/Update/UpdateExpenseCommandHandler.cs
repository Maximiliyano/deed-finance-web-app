using Deed.Application.Abstractions.Messaging;
using Deed.Application.Expenses.Specifications;
using Deed.Application.Tags.Specifications;
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

        // TODO include tagnames
        if (HasNoChanges(command, expense.Purpose))
        {
            return Result.Success();
        }

        if (command.Amount is not null)
        {
            var difference = expense.Amount - command.Amount.Value;

            expense.Capital.Balance += difference;

            expense.Amount = command.Amount.Value;
        }

        if (command.CapitalId.HasValue)
        {
            expense.CapitalId = command.CapitalId.Value;
        }
        
        // TODO execute tags from repository & complete
        if (command.TagNames is not null && command.TagNames.Any())
        {
            expense.Tags.Clear();
            expense.Tags.AddRange(command.TagNames.Select(t => new Domain.Entities.ExpenseTag()
            {
                Expense = expense,
                
            }));
        }

        expense.Purpose = command.Purpose;
        expense.PaymentDate = command.Date ?? expense.PaymentDate;

        if (command.CategoryId.HasValue)
        {
            expense.CategoryId = command.CategoryId.Value;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private bool HasNoChanges(UpdateExpenseCommand command, string? purpose) =>
        command.CategoryId is null &&
        command.CapitalId is null &&
        command.Amount is null &&
        command.Date is null &&
        command.Purpose == purpose;
}
