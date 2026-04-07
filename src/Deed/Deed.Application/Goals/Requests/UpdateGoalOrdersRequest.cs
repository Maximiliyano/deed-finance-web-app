using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Goals.Requests;

public sealed record UpdateGoalOrdersRequest(
    IEnumerable<GoalOrder> Goals
);
