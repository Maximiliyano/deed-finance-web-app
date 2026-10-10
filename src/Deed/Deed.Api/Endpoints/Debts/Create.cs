using Deed.Api.Extensions;
using Deed.Application.Debts.Commands.Create;
using Deed.Application.Debts.Requests;
using Deed.Domain.Enums;
using MediatR;

namespace Deed.Api.Endpoints.Debts;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/debts", async (CreateDebtRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CreateDebtCommand(
                request.Item, request.Amount, request.Currency,
                request.Source, request.Recipient, request.BorrowedAt,
                request.DeadlineAt, request.Note, request.CapitalId), ct))
                .Process())
            .AllowAnonymous()
            .WithTags(nameof(Debts));
    }
}
