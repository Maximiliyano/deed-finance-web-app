using Deed.Application.Abstractions.Messaging;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Incomes.Commands.Create;

internal sealed class CreateIncomeCommandHandler(
    ICapitalRepository capitalRepository,
    IIncomeRepository incomeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateIncomeCommand, int>
{
    public async Task<Result<int>> Handle(CreateIncomeCommand command, CancellationToken cancellationToken)
    {
        var capital = await capitalRepository.GetAsync(new CapitalByIdSpecification(command.CapitalId));

        if (capital is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound(nameof(capital)));
        }

        var income = command.ToEntity();

        capital.Balance += command.Amount;

        incomeRepository.Create(income);

        capitalRepository.Update(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return income.Id;
    }
}
