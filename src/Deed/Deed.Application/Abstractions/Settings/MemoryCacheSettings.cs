using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Abstractions.Settings;

public sealed class MemoryCacheSettings
{
    public int ExchangesTimespanInHours { get; init; }

    public int CategoriesTimespanInHours { get; init; }
}
