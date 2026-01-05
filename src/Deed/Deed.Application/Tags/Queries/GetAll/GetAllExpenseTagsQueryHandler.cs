using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Tags.Response;
using Deed.Application.Tags.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Application.Tags.Queries.GetAll;

internal sealed class GetAllExpenseTagsQueryHandler(IExpenseTagRepository expenseTagRepository)
    : IQueryHandler<GetAllExpenseTagsQuery, List<ExpenseTagResponse>>
{
    public async Task<Result<List<ExpenseTagResponse>>> Handle(
        GetAllExpenseTagsQuery query,
        CancellationToken cancellationToken)
    {
        var expenseTags = await expenseTagRepository
            .GetAllAsync(new ExpenseTagsByQuerySpefication(query.Term))
            .ConfigureAwait(false);

        return TagExtensions.ToResponse(expenseTags);
    }
}
