using Deed.Application.Abstractions.Messaging;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Incomes.Commands.Update;

internal sealed class UpdateIncomeCommandHandler(
    IIncomeRepository incomeRepository,
    ICapitalRepository capitalRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateIncomeCommand>
{
    public async Task<Result> Handle(UpdateIncomeCommand command, CancellationToken cancellationToken)
    {
        var income = await incomeRepository.GetAsync(new IncomeByIdSpecification(command.Id)).ConfigureAwait(false);

        if (income?.Capital is null || income?.Category is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(income)));
        }

        if (command.Amount.HasValue)
        {
            var difference = (float)(command.Amount - income.Amount);

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

        incomeRepository.Update(income);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
