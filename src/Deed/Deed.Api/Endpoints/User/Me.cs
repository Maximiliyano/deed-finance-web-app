using Deed.Application.Abstractions.Caching;
using Deed.Application.Auth;
using Deed.Application.UserSettings.Queries.Get;
using Deed.Application.UserSettings.Responses;
using Deed.Domain.Errors;
using MediatR;

namespace Deed.Api.Endpoints.User;

internal sealed class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/me", async (IUser user, ISender sender, ICacheService cacheService) =>
        {
            if (user.Name is null)
            {
                return Results.InternalServerError(DomainErrors.Anonymous.NameInvalid);
            }

            var userSettingRetrievalResult = await sender.Send(new GetUserSettingsQuery(user.Name));
            UserSettingsResponse? userSettings = userSettingRetrievalResult.Value;
            
            return Results.Ok(new UserResponse(
                user.ExternalId,
                user.Email,
                user.IsEmailVerified,
                user.Name,
                user.Roles,
                user.PictureUrl is null ? null : new Uri(Picture.Route, UriKind.Relative),
                userSettings));
        })
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}
