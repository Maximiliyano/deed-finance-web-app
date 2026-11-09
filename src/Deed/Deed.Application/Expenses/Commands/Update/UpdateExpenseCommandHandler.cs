using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Expenses.Specifications;
using Deed.Application.Tags;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Expenses.Commands.Update;

internal sealed class UpdateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    ICapitalRepository capitalRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateExpenseCommand>
{
    public async Task<Result> Handle(UpdateExpenseCommand command, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetAsync(new ExpenseByIdSpecification(command.Id, user.Name, includeCapital: true, includeCategory: true, includeTags: true, enableTracking: true), cancellationToken).ConfigureAwait(false);

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
        var newAmount = command.Amount ?? expense.Amount;

        if (command.CapitalId.HasValue && command.CapitalId.Value != expense.CapitalId)
        {
            var target = await capitalRepository
                .GetAsync(new CapitalByIdSpecification(command.CapitalId.Value, user.Name), cancellationToken)
                .ConfigureAwait(false);

            if (target is null)
            {
                return Result.Failure(DomainErrors.General.NotFound("capital"));
            }

            if (target.OnlyForSavings)
            {
                return Result.Failure(DomainErrors.Capital.ForSavingsOnly);
            }

            expense.Capital.Balance += expense.Amount;
            target.Balance -= newAmount;

            capitalRepository.Update(target);

            expense.CapitalId = target.Id;
            expense.Amount = newAmount;
            changed = true;
        }
        else if (newAmount != expense.Amount)
        {
            expense.Capital.Balance += expense.Amount - newAmount;
            expense.Amount = newAmount;
            changed = true;
        }

        if (command.TagNames is not null)
        {
            var desiredTags = TagLinkSynchronizer.Normalize(command.TagNames);

            if (!TagLinkSynchronizer.HasSameTags(expense.Tags, desiredTags))
            {
                await TagLinkSynchronizer
                    .SyncAsync(expense.Tags, desiredTags, tagRepository, tag => new ExpenseTag { Expense = expense, Tag = tag }, cancellationToken)
                    .ConfigureAwait(false);
                changed = true;
            }
        }

        if (command.Purpose != expense.Purpose)
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

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static bool HasNoChanges(UpdateExpenseCommand command, Expense expense) =>
        (command.CategoryId is null || command.CategoryId == expense.CategoryId) &&
        (command.CapitalId is null || command.CapitalId == expense.CapitalId) &&
        (command.Amount is null || command.Amount == expense.Amount) &&
        (command.Date is null || command.Date == expense.PaymentDate) &&
        command.Purpose == expense.Purpose &&
        (command.TagNames is null || TagLinkSynchronizer.HasSameTags(expense.Tags, TagLinkSynchronizer.Normalize(command.TagNames)));
}
