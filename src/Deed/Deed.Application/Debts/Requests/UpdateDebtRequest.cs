using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Enums;

namespace Deed.Application.Debts.Requests;

public sealed record UpdateDebtRequest(
    string Item,
    decimal Amount,
    string Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    bool IsPaid,
    int? PayFromCapitalId
);
