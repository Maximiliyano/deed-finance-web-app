using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Categories;
using Deed.Application.Categories.Specifications;
using Deed.Application.Incomes.Responses;
using Deed.Application.Incomes.Specifications;
using Deed.Domain.Enums;
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
        ArgumentException.ThrowIfNullOrEmpty(user.Name);

        var incomes = (await incomeRepository.GetAllAsync(new IncomesByQuerySpecification(user.Name), cancellationToken).ConfigureAwait(false)).ToResponses();
        return Result.Success(incomes);
    }
}
