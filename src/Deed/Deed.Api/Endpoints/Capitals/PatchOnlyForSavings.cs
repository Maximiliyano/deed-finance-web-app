
using Deed.Api.Extensions;
using Deed.Application.Capitals.Commands.PatchOnlyForSavings;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class PatchOnlyForSavings : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/capitals/{id:int}/savings-only", async (int id, bool onlyForSavings, ISender sender, CancellationToken cancellationToken) =>
            (await sender
                .Send(new PatchCapitalSetForSavingsCommand(id, onlyForSavings), cancellationToken))
                .Process())
            .WithTags(nameof(Capitals));
    }
}
