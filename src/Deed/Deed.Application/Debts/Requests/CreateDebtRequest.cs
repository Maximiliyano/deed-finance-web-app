using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Enums;

namespace Deed.Application.Debts.Requests;

public sealed record CreateDebtRequest(
    string Item,
    decimal Amount,
    CurrencyType Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    int? CapitalId);
