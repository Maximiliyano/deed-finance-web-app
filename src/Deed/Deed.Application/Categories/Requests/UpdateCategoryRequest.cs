using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Enums;

namespace Deed.Application.Categories.Requests;

public sealed record UpdateCategoryRequest(
    int Id,
    string Name,
    CategoryType Type,
    float PeriodAmount,
    PerPeriodType PeriodType
);
