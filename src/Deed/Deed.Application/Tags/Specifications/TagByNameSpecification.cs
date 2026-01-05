using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Domain.Entities;

namespace Deed.Application.Tags.Specifications;

internal sealed class TagByNameSpecification : BaseSpecification<Tag>
{
    public TagByNameSpecification(string name, bool tracking = false)
        : base(et => StringComparer.CurrentCultureIgnoreCase.Compare(et.Name, name) == 0)
    {
        Tracking = tracking;
    }
}
