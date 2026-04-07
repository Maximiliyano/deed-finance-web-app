using Deed.Api.Extensions;
using Deed.Application.Debts.Commands.Delete;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Debts;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/debts/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteDebtCommand(id), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(Debts));
    }
}
