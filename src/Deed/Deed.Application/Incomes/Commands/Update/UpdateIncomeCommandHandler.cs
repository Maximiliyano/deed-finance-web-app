using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Incomes.Commands.Update;

internal sealed class UpdateIncomeCommandHandler(
    IIncomeRepository incomeRepository,
    ICapitalRepository capitalRepository,
    ICategoryRepository categoryRepository,
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

        if ((command.CategoryId is null || command.CategoryId == income.CategoryId) &&
            (command.Amount is null || command.Amount == income.Amount) &&
            (command.Purpose is null || command.Purpose == income.Purpose) &&
            (command.PaymentDate is null || command.PaymentDate == income.PaymentDate) &&
            (command.TagNames is null || !command.TagNames.Any() ||
             command.TagNames.All(name => income.Tags.Exists(t => t.Tag.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))))
        {
            return Result.Success();
        }

        if (command.Amount.HasValue)
        {
            var difference = (decimal)(command.Amount - income.Amount);

            income.Capital.Balance += difference;

            income.Amount = command.Amount.Value;

            capitalRepository.Update(income.Capital);
        }

        income.Purpose = command.Purpose ?? income.Purpose;
        income.PaymentDate = command.PaymentDate ?? income.PaymentDate;

        if (command.CategoryId.HasValue)
        {
            income.CategoryId = command.CategoryId.Value;

            categoryRepository.Update(income.Category);
        }

        if (command.TagNames is not null && command.TagNames.Any())
        {
            income.Tags.Clear();
            income.Tags.AddRange(command.TagNames.Select(t => new IncomeTag
            {
                Income = income,
                Tag = new Tag { Name = t },
            }));
        }

        incomeRepository.Update(income);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
