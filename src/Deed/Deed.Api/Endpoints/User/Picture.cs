using System.Security.Cryptography;
using Deed.Application.Auth;
using Deed.Application.Auth.Picture;
using Microsoft.Net.Http.Headers;

namespace Deed.Api.Endpoints.User;

internal sealed class Picture : IEndpoint
{
    public const string Route = "api/users/me/picture";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Route, async (
            IUser user,
            IProfilePictureService pictures,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            if (user.PictureUrl is null)
            {
                return Results.NotFound();
            }

            var picture = await pictures.GetAsync(user.PictureUrl, cancellationToken);

            if (picture is null)
            {
                return Results.NotFound();
            }

            var data = picture.Data.ToArray();
            var tag = $"\"{Convert.ToHexString(SHA256.HashData(data))[..16]}\"";

            response.Headers.CacheControl = "private, max-age=86400";

            return Results.File(data, picture.ContentType, entityTag: new EntityTagHeaderValue(tag));
        })
        .RequireAuthorization()
        .WithTags(nameof(User));
    }
}
