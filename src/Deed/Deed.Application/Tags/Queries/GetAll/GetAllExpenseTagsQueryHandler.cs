using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Tags.Response;
using Deed.Application.Tags.Specifications;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Tags.Queries.GetAll;

internal sealed class GetAllExpenseTagsQueryHandler(IExpenseTagRepository expenseTagRepository, IUser user)
    : IQueryHandler<GetAllExpenseTagsQuery, List<ExpenseTagResponse>>
{
    public async Task<Result<List<ExpenseTagResponse>>> Handle(
        GetAllExpenseTagsQuery query,
        CancellationToken cancellationToken)
    {
        var expenseTags = await expenseTagRepository
            .GetAllAsync(new ExpenseTagsByQuerySpefication(query.Term, user.Name ?? string.Empty), cancellationToken)
            .ConfigureAwait(false);

        return TagExtensions.ToResponse(expenseTags);
    }
}
