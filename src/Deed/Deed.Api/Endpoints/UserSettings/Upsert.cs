using Deed.Api.Extensions;
using Deed.Application.UserSettings.Commands.Upsert;
using Deed.Domain.Results;
using MediatR;

namespace Deed.Api.Endpoints.UserSettings;

internal sealed class Upsert : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/user-settings", async (UpsertUserSettingsRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpsertUserSettingsCommand(
                request.Salary,
                request.Currency,
                request.BalanceReminderEnabled,
                request.BalanceReminderCron,
                request.ExpenseReminderEnabled,
                request.ExpenseReminderCron,
                request.DebtReminderEnabled,
                request.DebtReminderCron,
                request.EmailNotificationsEnabled), ct))
                .Process(ResultType.NoContent))
            .AllowAnonymous()
            .WithTags(nameof(UserSettings));
    }
}
