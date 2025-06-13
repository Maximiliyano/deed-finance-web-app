using Deed.Application.Abstractions.Messaging;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Providers;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Expenses.Commands.Update;

internal sealed class UpdateExpenseCommandHandler(
    IDateTimeProvider dateTimeProvider,
    ICapitalRepository capitalRepository,
    ICategoryRepository categoryRepository,
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateExpenseCommand>
{
    public async Task<Result> Handle(UpdateExpenseCommand command, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetAsync(new ExpenseByIdSpecification(command.Id));

        if (expense is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(expense)));
        }

        if (command.Amount is not null)
        {
            var difference = expense.Amount - command.Amount.Value;

            expense.Capital!.Balance += difference; // TODO !

            expense.Amount = command.Amount.Value;

            capitalRepository.Update(expense.Capital);
        }

        expense.Purpose = command.Purpose ?? expense.Purpose;
        expense.PaymentDate = command.Date ?? dateTimeProvider.UtcNow;

        if (command.CategoryId.HasValue)
        {
            expense.CategoryId = command.CategoryId.Value;

            categoryRepository.Update(expense.Category!); // TODO !
        }

        expenseRepository.Update(expense);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
