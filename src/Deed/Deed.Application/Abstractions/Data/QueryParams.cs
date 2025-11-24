using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Abstractions.Data;

public sealed record QueryParams(
    string? SearchTerm,
    string? SortBy,
    string? SortDirection,
    string? FilterBy
);
