
using Deed.Api.Extensions;
using Deed.Application.Auth;
using Deed.Application.Tags.Commands.Delete;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Tags;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/tags/{id:int}", async (int id, ISender sender, CancellationToken token) =>
            (await sender
                .Send(new DeleteTagCommand(id), token))
                .Process(ResultType.NoContent))
            .RequireAuthorization(AuthConstants.AdminOnlyPolicy)
            .WithTags(nameof(Tags));
    }
}
