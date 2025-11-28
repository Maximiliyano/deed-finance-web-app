using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;

namespace Deed.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password)
: ICommand<string>;
