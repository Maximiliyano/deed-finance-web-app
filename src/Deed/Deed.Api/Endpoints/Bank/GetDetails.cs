using Deed.Api.Extensions;
using Deed.Application.Bank.Queries.GetAllBankDetails;
using MediatR;

namespace Deed.Api.Endpoints.Bank;

internal sealed class GetDetails : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bank/details", async (ISender sender, CancellationToken ct) =>
                (await sender
                    .Send(new GetBankDetailsQuery(), ct))
                    .Process())
            .AllowAnonymous()
            .WithTags(nameof(Bank));
    }
}
