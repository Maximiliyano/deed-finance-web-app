using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Incomes.Responses;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Incomes.Queries.GetAll;

internal sealed class GetIncomesQueryHandler(
    IUser user,
    IIncomeRepository incomeRepository
) : IQueryHandler<GetIncomesQuery, IEnumerable<IncomeResponse>>
{
    public async Task<Result<IEnumerable<IncomeResponse>>> Handle(GetIncomesQuery query, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(user.Id);

        var incomes = (await incomeRepository.GetAllAsync(new IncomesByQuerySpecification(user.Id, ToOffset(query.PeriodStart), ToOffset(query.PeriodEnd)), cancellationToken).ConfigureAwait(false)).ToResponses();
        return Result.Success(incomes);
    }

    private static DateTimeOffset? ToOffset(DateTime? value) =>
        value.HasValue ? new DateTimeOffset(value.Value) : null;
}
