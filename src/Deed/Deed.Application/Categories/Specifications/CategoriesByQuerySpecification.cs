using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Deed.Application.Categories.Specifications;

internal sealed class CategoriesByQuerySpecification
    : BaseSpecification<Category>
{
    public CategoriesByQuerySpecification(IEnumerable<int> ids, bool tracking = false, CategoryType? type = null, bool? includeDeleted = null)
        : base(GetCriteria(ids, type))
    {
        Tracking = tracking;

        if (includeDeleted.HasValue)
        {
            IgnoreQueryFilter = includeDeleted;
        }
    }

    private static Expression<Func<Category, bool>>? GetCriteria(IEnumerable<int> ids, CategoryType? type)
    {
        return c =>
            (!type.HasValue || c.Type == type.Value) &&
            (!ids.Any() || ids.Contains(c.Id));
    }
}
