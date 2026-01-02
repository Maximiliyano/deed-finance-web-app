using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Tags.Response;

public sealed record ExpenseTagResponse(
    int TagId,
    int ExpenseId,
    string Name
);
