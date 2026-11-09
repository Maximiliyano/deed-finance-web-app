using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Incomes.Specifications;
using Deed.Application.Tags;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Incomes.Commands.Update;

internal sealed class UpdateIncomeCommandHandler(
    IIncomeRepository incomeRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<UpdateIncomeCommand>
{
    public async Task<Result> Handle(UpdateIncomeCommand command, CancellationToken cancellationToken)
    {
        var income = await incomeRepository.GetAsync(
            new IncomeByIdSpecification(command.Id, user.Name, includeTags: command.TagNames is not null, enableTracking: true), cancellationToken)
            .ConfigureAwait(false);

        if (income?.Capital is null || income?.Category is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(income)));
        }

        if (HasNoChanges(command, income))
        {
            return Result.Success();
        }

        if (command.Amount.HasValue)
        {
            income.Capital.Balance += command.Amount.Value - income.Amount;
            income.Amount = command.Amount.Value;
        }

        income.Purpose = command.Purpose;
        income.PaymentDate = command.PaymentDate ?? income.PaymentDate;

        if (command.CategoryId.HasValue)
        {
            income.CategoryId = command.CategoryId.Value;
        }

        if (command.TagNames is not null)
        {
            await TagLinkSynchronizer
                .SyncAsync(income.Tags, TagLinkSynchronizer.Normalize(command.TagNames), tagRepository, tag => new IncomeTag { Income = income, Tag = tag }, cancellationToken)
                .ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private static bool HasNoChanges(UpdateIncomeCommand command, Income income) =>
        (command.CategoryId is null || command.CategoryId == income.CategoryId) &&
        (command.Amount is null || command.Amount == income.Amount) &&
        command.Purpose == income.Purpose &&
        (command.PaymentDate is null || command.PaymentDate == income.PaymentDate) &&
        (command.TagNames is null || TagLinkSynchronizer.HasSameTags(income.Tags, TagLinkSynchronizer.Normalize(command.TagNames)));
}
