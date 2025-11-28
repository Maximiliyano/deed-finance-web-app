using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Abstractions.Settings;
using Deed.Domain.Errors;
using Deed.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Deed.Application.Auth.Commands.Login;

internal sealed class LoginCommandHandler()
    : ICommandHandler<LoginCommand, string>
{
    public Task<Result<string>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // use identity server once again here
        

        return Task.FromResult(Result.Success(string.Empty));
    }
}
