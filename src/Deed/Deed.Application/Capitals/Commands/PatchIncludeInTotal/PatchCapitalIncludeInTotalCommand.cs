using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Capitals.Commands.PatchIncludeInTotal;

public sealed record PatchCapitalIncludeInTotalCommand(
    int Id,
    bool IncludeInTotal
) : ICommand;
