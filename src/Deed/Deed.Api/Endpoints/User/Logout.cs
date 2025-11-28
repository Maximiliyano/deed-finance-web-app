
using System.IdentityModel.Tokens.Jwt;
using Deed.Api.Extensions;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth.Commands.Login;
using Deed.Infrastructure.Persistence.Constants;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;

namespace Deed.Api.Endpoints.User;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/logout", async (HttpContext context, IOptions<WebUrlSettings> options) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (context.User.Identity?.IsAuthenticated == true)
            {
                await context.SignOutAsync(Auth0Constants.AuthenticationScheme, new AuthenticationProperties
                {
                    RedirectUri = options.Value.UIUrl
                });
            }
            else
            {
                context.Response.Redirect(options.Value.UIUrl); // fallback
            }
        })
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}
