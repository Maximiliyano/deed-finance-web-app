using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Tags.Commands.Create;

internal sealed class CreateExpenseTagCommandHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateExpenseTagCommand, int>
{
    public async Task<Result<int>> Handle(CreateExpenseTagCommand command, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetAsync(new ExpenseByIdSpecification(command.ExpenseId, enableTracking: true));

        if (expense is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound(nameof(expense)));
        }

        var tag = new Tag()
        {
            Name = command.Name
        };
        var expenseTag = new ExpenseTag()
        {
            Id = command.ExpenseId,
            Expense = expense,
            TagId = tag.Id,
            Tag = tag
        };

        expense.Tags.Add(expenseTag);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(expenseTag.Id);
    }
}
