using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Capitals.Commands.PatchOnlyForSavings;

public sealed record PatchCapitalSetForSavingsCommand(
    int Id, 
    bool OnlyForSavings
) : ICommand;
