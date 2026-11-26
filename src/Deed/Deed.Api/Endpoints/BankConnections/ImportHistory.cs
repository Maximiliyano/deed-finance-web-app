using Deed.Api.Extensions;
using Deed.Application.BankConnections.Commands.ImportBankHistory;
using MediatR;

namespace Deed.Api.Endpoints.BankConnections;

internal sealed class ImportHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bank-connections/history", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new ImportBankHistoryCommand(), ct)).Process())
            .RequireAuthorization()
            .WithTags(nameof(BankConnections));
    }
}
