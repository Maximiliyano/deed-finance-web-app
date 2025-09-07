using Deed.Api.Extensions;
using Deed.Application.Capitals.Commands.Create;
using Deed.Application.Capitals.Requests;
using Deed.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Deed.Api.Endpoints.Capitals;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/capitals", async (CreateCapitalRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new CreateCapitalCommand(request.Name, request.Balance, request.Currency, request.IncludeInTotal), ct))
                .Process())
            .WithTags(nameof(Capitals));
    }
}
