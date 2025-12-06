
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Deed.Api.Extensions;
using Deed.Application.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;

namespace Deed.Api.Endpoints.User;

internal sealed class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/me", (HttpContext context) =>
        {
            var user = context.User;
            if (!bool.TryParse(user.Claims.FirstOrDefault(x => x.Type == "email_verified")?.Value, out var emailVerified))
            {
                return Results.BadRequest("Email verified prop absent");
            }
            return Results.Ok(new UserResponse(
                user.Claims.FirstOrDefault(x => x.Type == ClaimValueTypes.Email)?.Value,
                emailVerified,
                user.Claims.FirstOrDefault(x => x.Type == "name")?.Value,
                new Uri(user.Claims.FirstOrDefault(x => x.Type == "picture")?.Value ?? string.Empty)
            ));
        })
        .RequireAuthorization(new AuthorizeAttribute
        {
            AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme
        })
        .WithTags(nameof(User));
    }
}

internal sealed record UserResponse(
    string? Email,
    bool? EmailVerified,
    string? Fullname,
    Uri PictureUrl
);
