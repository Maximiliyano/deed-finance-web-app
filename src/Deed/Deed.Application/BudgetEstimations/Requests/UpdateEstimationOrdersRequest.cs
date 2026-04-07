using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.BudgetEstimations.Requests;

public sealed record UpdateEstimationOrdersRequest(
    IEnumerable<EstimationOrder> Estimations
);
