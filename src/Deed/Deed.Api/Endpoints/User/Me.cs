using Deed.Application.Auth;

namespace Deed.Api.Endpoints.User;

internal sealed class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/me", (IUser user) =>
            Results.Ok(new UserResponse(
                user.Email, user.IsEmailVerified, user.Name, user.PictureUrl)))
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}
