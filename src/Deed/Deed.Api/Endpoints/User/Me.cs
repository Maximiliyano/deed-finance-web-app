using Deed.Api.Extensions;
using Deed.Application.Auth;
using Deed.Application.UserSettings.Queries.Get;
using Deed.Application.UserSettings.Responses;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using MediatR;

namespace Deed.Api.Endpoints.User;

internal sealed class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/me", async (IUser user, ISender sender) =>
        {
            if (user.Name is null)
            {
                return Results.InternalServerError(DomainErrors.Anonymous.NameInvalid);
            }

            var userSettingRetrievalResult = await sender.Send(new GetUserSettingsQuery(user.Name));
            
            if (!userSettingRetrievalResult.IsSuccess)
            {
                return userSettingRetrievalResult.Process();
            }

            return Results.Ok(new UserResponse(
                user.ExternalId,
                user.Email,
                user.IsEmailVerified,
                user.Name,
                user.PictureUrl,
                userSettingRetrievalResult.Value));
        })
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}
