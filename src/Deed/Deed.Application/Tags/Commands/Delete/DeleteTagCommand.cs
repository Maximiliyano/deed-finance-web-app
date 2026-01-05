using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Tags.Commands.Delete;

public sealed record DeleteTagCommand(
    int Id
) : ICommand;
