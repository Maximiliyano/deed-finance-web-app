using Deed.Api.Extensions;
using Deed.Application.Capitals.Commands.Update;
using Deed.Application.Capitals.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/capitals/{id:int}", async (int id, UpdateCapitalRequest request, ISender sender) =>
            (await sender
                .Send(new UpdateCapitalCommand(id, request.Name, request.Balance, request.Currency, request.IncludeInTotal)))
                .Process())
            .WithTags(nameof(Capitals));
    }
}
