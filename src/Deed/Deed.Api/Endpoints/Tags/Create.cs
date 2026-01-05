
using Deed.Api.Extensions;
using Deed.Application.Tags.Commands.Create;
using Deed.Application.Tags.Requests;
using MediatR;

namespace Deed.Api.Endpoints.Tags;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/tags", async (CreateExpenseTagRequest request, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new CreateExpenseTagCommand(request.ExpenseId, request.Name), ct))
            .Process())
            .RequireAuthorization()
            .WithTags(nameof(Tags));
    }
}
