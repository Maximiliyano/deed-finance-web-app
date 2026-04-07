using Deed.Api.Extensions;
using Deed.Application.UserSettings.Queries.Get;
using MediatR;

namespace Deed.Api.Endpoints.UserSettings;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/user-settings", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetUserSettingsQuery(), ct)).Process())
            .AllowAnonymous()
            .WithTags(nameof(UserSettings));
    }
}
