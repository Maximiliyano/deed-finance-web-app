
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Deed.Api.Extensions;
using Deed.Application.Auth;
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
        app.MapGet("api/users/me", (IUser user) =>
        {
            if (!user.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UserResponse(
                user.Email, user.IsEmailVerified, user.Name, user.PictureUrl));
        })
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}
