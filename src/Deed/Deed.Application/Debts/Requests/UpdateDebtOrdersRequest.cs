using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Debts.Requests;

public sealed record UpdateDebtOrdersRequest(
    IEnumerable<DebtOrder> Debts
);
