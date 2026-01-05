using Deed.Application.Abstractions.Messaging;
using Deed.Application.Tags.Response;

namespace Deed.Application.Tags.Queries.GetAll;

public sealed record GetAllExpenseTagsQuery(
    string? Term = null
) : IQuery<List<ExpenseTagResponse>>;
