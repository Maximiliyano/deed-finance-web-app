using Deed.Application.Abstractions.Messaging;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Expenses.Commands.Delete;

internal sealed class DeleteExpenseCommandHandler(
    ICapitalRepository capitalRepository,
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteExpenseCommand>
{
    public async Task<Result> Handle(DeleteExpenseCommand command, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetAsync(new ExpenseByIdSpecification(command.Id, includeCapital: true)).ConfigureAwait(false);

        if (expense?.Capital is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(expense)));
        }

        expense.Capital.Balance += expense.Amount;

        capitalRepository.Update(expense.Capital);

        expenseRepository.Delete(expense);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
