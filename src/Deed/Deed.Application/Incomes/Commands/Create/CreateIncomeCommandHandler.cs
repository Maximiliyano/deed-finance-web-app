using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Incomes.Specifications;
using Deed.Application.Tags.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Incomes.Commands.Create;

internal sealed class CreateIncomeCommandHandler(
    IUser user,
    ICapitalRepository capitalRepository,
    IIncomeRepository incomeRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateIncomeCommand, int>
{
    public async Task<Result<int>> Handle(CreateIncomeCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await incomeRepository.CountAsync(
                new IncomesByQuerySpecification(user.Name!), cancellationToken).ConfigureAwait(false);
            if (count >= AnonymousConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var capital = await capitalRepository.GetAsync(new CapitalByIdSpecification(command.CapitalId), cancellationToken).ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound(nameof(capital)));
        }

        var income = command.ToEntity();

        foreach (var tagName in command.TagNames ?? [])
        {
            var tag = await tagRepository.GetAsync(new TagByNameSpecification(tagName, true), cancellationToken).ConfigureAwait(false);

            if (tag is null)
            {
                tagRepository.Create(new()
                {
                    Name = tagName,
                    IncomeTags = [new() { Income = income }]
                });
                continue;
            }

            income.Tags.Add(new() { Tag = tag });
        }

        capital.Balance += command.Amount;

        incomeRepository.Create(income);

        capitalRepository.Update(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Income {Id} successfully created", income.Id);

        return Result.Success(income.Id);
    }
}
