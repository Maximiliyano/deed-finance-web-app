using Deed.Api.Extensions;
using Deed.Application.Capitals.Commands.PatchIncludeInTotal;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class PatchIncludeTotal : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/capitals/{id:int}/include-in-total", async (int id, bool includeInTotal, ISender sender, CancellationToken cancellationToken) =>
            (await sender
                .Send(new PatchCapitalIncludeInTotalCommand(id, includeInTotal), cancellationToken))
                .Process())
            .WithTags(nameof(Capitals));
    }
}
