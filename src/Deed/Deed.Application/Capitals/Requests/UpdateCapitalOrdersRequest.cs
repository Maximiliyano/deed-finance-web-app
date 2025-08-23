using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Capitals.Requests;

public sealed record UpdateCapitalOrdersRequest(
    IEnumerable<CapitalOrder> Capitals
);
