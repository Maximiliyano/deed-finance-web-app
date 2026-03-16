using Deed.Api.Extensions;
using Deed.Application.Debts.Commands.Create;
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
            .RequireAuthorization()
            .WithTags(nameof(Debts));
    }
}

internal sealed record CreateDebtRequest(
    string Item,
    decimal Amount,
    CurrencyType Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    int? CapitalId);
