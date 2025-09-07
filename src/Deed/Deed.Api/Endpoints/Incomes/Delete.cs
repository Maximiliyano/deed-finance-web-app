using Deed.Api.Extensions;
using Deed.Application.Incomes.Commands.Delete;
using MediatR;

namespace Deed.Api.Endpoints.Incomes;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/incomes/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender
                .Send(new DeleteIncomeCommand(id), ct))
                .Process())
            .WithTags(nameof(Incomes));
    }
}
