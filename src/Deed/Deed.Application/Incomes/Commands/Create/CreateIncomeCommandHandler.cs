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

        var income = command.ToEntity();

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
                    income.Tags.Add(new() { Tag = tag });
                    continue;
                }

                tagRepository.Create(new()
                {
                    Name = tagName,
                    IncomeTags = [new() { Income = income }]
                });
            }
        }

        capital.Balance += command.Amount;

        incomeRepository.Create(income);

        capitalRepository.Update(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Income {Id} successfully created", income.Id);

        return Result.Success(income.Id);
    }
}
