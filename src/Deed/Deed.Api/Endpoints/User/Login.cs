
using System.IdentityModel.Tokens.Jwt;
using Deed.Api.Extensions;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth.Commands.Login;
using Deed.Infrastructure.Persistence.Constants;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;

namespace Deed.Api.Endpoints.User;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/auth/login", (IOptions<WebUrlSettings> authSettings) => Results.Challenge(new AuthenticationProperties()
        {
            RedirectUri = $"{authSettings.Value.UIUrl}/profile"
        }, [AuthConstants.AuthenticationScheme]))
            .WithTags(nameof(User))
            .AllowAnonymous();

        app.MapGet("api/auth/callback", () => Results.Ok())
            .WithTags(nameof(User))
            .AllowAnonymous();
    }
}
