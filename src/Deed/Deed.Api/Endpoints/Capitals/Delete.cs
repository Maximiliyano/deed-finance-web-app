using Deed.Domain.Results;
using Deed.Api.Extensions;
using Deed.Application.Capitals.Commands.Delete;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/capitals/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new DeleteCapitalCommand(id), ct))
                .Process(ResultType.NoContent))
            .WithTags(nameof(Capitals));
    }
}
