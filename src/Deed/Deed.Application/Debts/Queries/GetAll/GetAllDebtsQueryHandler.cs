using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Debts.Responses;
using Deed.Application.Debts.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Debts.Queries.GetAll;

internal sealed class GetAllDebtsQueryHandler(IDebtRepository repository, IUser user)
    : IQueryHandler<GetAllDebtsQuery, IEnumerable<DebtResponse>>
{
    public async Task<Result<IEnumerable<DebtResponse>>> Handle(
        GetAllDebtsQuery query,
        CancellationToken cancellationToken)
    {
        var debts = await repository
            .GetAllAsync(new DebtsByUserSpecification(user.Name!), cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(debts.ToResponses());
    }
}
