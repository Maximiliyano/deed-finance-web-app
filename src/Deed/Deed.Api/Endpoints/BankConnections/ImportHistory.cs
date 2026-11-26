using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.ImportBankHistory;
using Deed.Application.BankConnections.Requests;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class ImportHistory : IEndpoint
{
    private const int DefaultDays = 30;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/history", async (ImportBankHistoryRequest? request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ImportBankHistoryCommand(request?.From ?? DateTimeOffset.UtcNow.AddDays(-DefaultDays), request?.To), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
