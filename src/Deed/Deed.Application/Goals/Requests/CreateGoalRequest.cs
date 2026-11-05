using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Domain.Enums;

namespace Deed.Application.Goals.Requests;

public sealed record CreateGoalRequest(
    string Title,
    decimal TargetAmount,
    string Currency,
    decimal CurrentAmount,
    DateTimeOffset? Deadline,
    string? Note
);
