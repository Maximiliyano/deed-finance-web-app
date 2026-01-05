using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Tags.Response;
using Deed.Domain.Entities;

namespace Deed.Application.Tags;

internal static class TagExtensions
{
    public static List<ExpenseTagResponse> ToResponse(this IEnumerable<ExpenseTag> expenseTags)
        => [.. expenseTags.Select(et => et.ToResponse())];

    public static ExpenseTagResponse ToResponse(this ExpenseTag expenseTag)
        => new(
            expenseTag.TagId,
            expenseTag.Id,
            expenseTag.Tag.Name
        );
}
