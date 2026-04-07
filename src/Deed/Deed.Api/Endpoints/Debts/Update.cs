using Deed.Api.Extensions;
using Deed.Application.Debts.Commands.Update;
using Deed.Domain.Enums;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.Debts;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/debts/{id:int}", async (int id, UpdateDebtRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateDebtCommand(
                id, request.Item, request.Amount, request.Currency,
                request.Source, request.Recipient, request.BorrowedAt,
                request.DeadlineAt, request.Note, request.IsPaid,
                request.PayFromCapitalId), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(Debts));
    }
}

internal sealed record UpdateDebtRequest(
    string Item,
    decimal Amount,
    CurrencyType Currency,
    string Source,
    string Recipient,
    DateTimeOffset BorrowedAt,
    DateTimeOffset? DeadlineAt,
    string? Note,
    bool IsPaid,
    int? PayFromCapitalId);
