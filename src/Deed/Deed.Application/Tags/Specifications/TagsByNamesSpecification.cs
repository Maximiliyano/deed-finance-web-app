using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Tags.Specifications;

internal sealed class TagsByNamesSpecification : BaseSpecification<Tag>
{
    public TagsByNamesSpecification(IEnumerable<string> names, bool tracking = false)
        : base(BuildCriteria(names.ToList()))
    {
        Tracking = tracking;
    }

    private static Expression<Func<Tag, bool>> BuildCriteria(List<string> names)
        => tag => names.Contains(tag.Name);
}
