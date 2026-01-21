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
    IIncomeRepository incomeRepository,
    ICategoryRepository categoryRepository,
    ICapitalRepository capitalRepository
) : IQueryHandler<GetIncomesQuery, IncomesResponse>
{
    public async Task<Result<IncomesResponse>> Handle(GetIncomesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(user.Name);

        var incomes = (await incomeRepository.GetAllAsync(new IncomesByQuerySpecification(user.Name)).ConfigureAwait(false)).ToResponses();
        var categories = (await categoryRepository.GetAllAsync(new CategoriesByQuerySpecification([], type: CategoryType.Incomes)).ConfigureAwait(false)).ToResponses();
        var capitals = (await capitalRepository.GetAllAsync(new CapitalsByQueryParamsSpecification(user.Name)).ConfigureAwait(false)).ToResponses();

        return Result.Success(new IncomesResponse(incomes, categories, capitals));
    }
}
