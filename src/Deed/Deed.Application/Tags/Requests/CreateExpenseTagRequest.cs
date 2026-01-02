using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Entities;

namespace Deed.Application.Tags.Requests;

public sealed record CreateExpenseTagRequest(
    int ExpenseId,
    string Name
);
