using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Expenses.Specifications;
using Deed.Application.Tags.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Expenses.Commands.Create;

internal sealed class CreateExpenseCommandHandler(
    IUser user,
    ICapitalRepository capitalRepository,
    IExpenseRepository expenseRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateExpenseCommand, int>
{
    public async Task<Result<int>> Handle(CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await expenseRepository.CountAsync(
                new ExpenseByQueriesSpecification(user.Name!), cancellationToken).ConfigureAwait(false);
            
            if (count >= AuthConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var capital = await capitalRepository.GetAsync(new CapitalByIdSpecification(command.CapitalId), cancellationToken).ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound(nameof(capital)));
        }

        if (capital.OnlyForSavings)
        {
            return Result.Failure<int>(DomainErrors.Capital.ForSavingsOnly);
        }

        var expense = command.ToEntity();

        var tagNames = (command.TagNames ?? []).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();

        if (tagNames.Count > 0)
        {
            var existingByName = (await tagRepository
                    .GetAllAsync(new TagsByNamesSpecification(tagNames, tracking: true), cancellationToken)
                    .ConfigureAwait(false))
                .ToDictionary(t => t.Name, StringComparer.CurrentCultureIgnoreCase);

            foreach (var tagName in tagNames)
            {
                if (existingByName.TryGetValue(tagName, out var tag))
                {
                    expense.Tags.Add(new ()
                    {
                        Tag = tag
                    });
                    continue;
                }

                tagRepository.Create(new ()
                {
                    Name = tagName,
                    ExpenseTags =
                    [
                        new ()
                        {
                            Expense = expense
                        }
                    ]
                });
            }
        }


        capital.Balance -= expense.Amount;

        capitalRepository.Update(capital);

        expenseRepository.Create(expense);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Expense {Id} successfully created", expense.Id);

        return Result.Success(expense.Id);
    }
}
