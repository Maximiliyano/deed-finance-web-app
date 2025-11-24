
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
        app.MapGet("api/auth/login", async (HttpContext context, IOptions<WebUrlSettings> options) =>
            await context.ChallengeAsync(Auth0Constants.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = $"{options.Value.UIUrl}/profile"
            }))
            .WithTags(nameof(User));

        app.MapGet("/api/auth/callback", (HttpContext context) =>
            // TODO: The middleware automatically processes the response and signs in the user
            context.Response.Redirect("http://localhost:4200/profile"))
        .AllowAnonymous();
    }
}
