using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Enums;

namespace Deed.Application.Expenses.Responses;

public sealed record CategoryExpenseResponse(
    int CategoryId,
    string Name,
    decimal CategorySum,
    decimal Percentage,
    decimal PlannedPeriodAmount,
    string PeriodType,
    IEnumerable<ExpenseResponse> Expenses
);
