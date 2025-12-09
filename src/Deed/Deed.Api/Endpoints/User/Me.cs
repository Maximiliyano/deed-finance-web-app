
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
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var email = user.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var emailVerified = bool.TryParse(user.FindFirst(c => c.Type == "email_verified")?.Value, out var v) && v;
            var name = user.FindFirst(c => c.Type == "name")?.Value;
            var picture = user.FindFirst(c => c.Type == "picture")?.Value ?? string.Empty;
            var userSid = user.FindFirst(c => c.Type == "sid")?.Value;

            var resp = new UserResponse(userSid, email, emailVerified, name, new Uri(picture));
            return Results.Ok(resp);
        })
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}

internal sealed record UserResponse(
    string? Sid,
    string? Email,
    bool? EmailVerified,
    string? Fullname,
    Uri PictureUrl
);
