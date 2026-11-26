using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Categories.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.BankConnections.Commands.UpdateBankConnection;

internal sealed class UpdateBankConnectionCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    ICapitalRepository capitalRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBankConnectionCommand>
{
    public async Task<Result> Handle(UpdateBankConnectionCommand command, CancellationToken cancellationToken)
    {
        var connection = await repository
            .GetAsync(new BankConnectionByIdSpecification(command.Id, user.Name!, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        if (connection is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(connection)));
        }

        if (HasNoChanges(command, connection))
        {
            return Result.Success();
        }

        if (command.CapitalId is { } capitalId)
        {
            var capital = await capitalRepository
                .GetAsync(new CapitalByIdSpecification(capitalId, user.Name), cancellationToken)
                .ConfigureAwait(false);

            if (capital is null)
            {
                return Result.Failure(DomainErrors.General.NotFound(nameof(capital)));
            }
        }

        var expenseCheck = await CheckCategoryAsync(command.ExpenseCategoryId, CategoryType.Expenses, cancellationToken).ConfigureAwait(false);

        if (!expenseCheck.IsSuccess)
        {
            return expenseCheck;
        }

        var incomeCheck = await CheckCategoryAsync(command.IncomeCategoryId, CategoryType.Incomes, cancellationToken).ConfigureAwait(false);

        if (!incomeCheck.IsSuccess)
        {
            return incomeCheck;
        }

        connection.Label = BankConnectionExtensions.NormalizeLabel(command.Label) ?? connection.Label;
        connection.CapitalId = command.CapitalId;
        connection.AutoImport = command.CapitalId is not null && command.AutoImport;
        connection.ExpenseCategoryId = command.ExpenseCategoryId;
        connection.IncomeCategoryId = command.IncomeCategoryId;

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }

    private async Task<Result> CheckCategoryAsync(int? id, CategoryType expected, CancellationToken cancellationToken)
    {
        if (id is not { } categoryId)
        {
            return Result.Success();
        }

        var category = await categoryRepository
            .GetAsync(new CategoryByIdSpecification(categoryId), cancellationToken)
            .ConfigureAwait(false);

        if (category is null)
        {
            return Result.Failure(DomainErrors.General.NotFound(nameof(category)));
        }

        return category.Type == expected ? Result.Success() : Result.Failure(DomainErrors.Bank.CategoryTypeMismatch);
    }

    private static bool HasNoChanges(UpdateBankConnectionCommand command, BankConnection connection)
        => (BankConnectionExtensions.NormalizeLabel(command.Label) ?? connection.Label) == connection.Label
            && command.CapitalId == connection.CapitalId
            && (command.CapitalId is not null && command.AutoImport) == connection.AutoImport
            && command.ExpenseCategoryId == connection.ExpenseCategoryId
            && command.IncomeCategoryId == connection.IncomeCategoryId;
}
