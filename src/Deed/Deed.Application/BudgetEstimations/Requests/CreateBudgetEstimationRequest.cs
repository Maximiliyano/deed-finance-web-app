using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Enums;

namespace Deed.Application.BudgetEstimations.Requests;

public sealed record CreateBudgetEstimationRequest(
    string Description,
    decimal BudgetAmount,
    CurrencyType BudgetCurrency,
    int? CapitalId
);
