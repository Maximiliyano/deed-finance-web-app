using Deed.Application.Abstractions.Messaging;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Expenses.Commands.Update;

internal sealed class UpdateExpenseCommandHandler(
    ICapitalRepository capitalRepository,
    ICategoryRepository categoryRepository,
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateExpenseCommand>
{
    public async Task<Result> Handle(UpdateExpenseCommand command, CancellationToken cancellationToken) // TODO update tests
    {
        var expense = await expenseRepository.GetAsync(new ExpenseByIdSpecification(command.Id, true, true)).ConfigureAwait(false);

        if (expense?.Capital is null || expense?.Category is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(expense)));
        }

        if (HasNoChanges(command, expense.Purpose))
        {
            return Result.Success();
        }

        if (expense.Capital.OnlyForSavings)
        {
            return Result.Failure(DomainErrors.Capital.ForSavingsOnly);
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

        if (command.Amount.HasValue || command.CapitalId.HasValue)
        {
            capitalRepository.Update(expense.Capital);
        }

        expense.Purpose = command.Purpose;
        expense.PaymentDate = command.Date ?? expense.PaymentDate;

        if (command.CategoryId.HasValue)
        {
            expense.CategoryId = command.CategoryId.Value;

            categoryRepository.Update(expense.Category);
        }

        expenseRepository.Update(expense);

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
