using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Expenses.Responses;
using Deed.Application.Expenses.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Expenses.Queries.GetById;

internal sealed class GetExpenseByIdQueryHandler(IExpenseRepository repository, IUser user)
    : IQueryHandler<GetExpenseByIdQuery, ExpenseResponse>
{
    public async Task<Result<ExpenseResponse>> Handle(GetExpenseByIdQuery query, CancellationToken cancellationToken)
    {
        var expense = await repository.GetAsync(new ExpenseByIdSpecification(query.Id, user.Name), cancellationToken).ConfigureAwait(false);

        if (expense is null)
        {
            return Result.Failure<ExpenseResponse>(DomainErrors.General.NotFound(nameof(expense)));
        }

        return expense.ToResponse();
    }
}
