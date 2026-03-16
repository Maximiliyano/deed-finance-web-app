using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.UserSettings.Responses;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.UserSettings.Queries.Get;

internal sealed class GetUserSettingsQueryHandler(IUserSettingsRepository repository, IUser user)
    : IQueryHandler<GetUserSettingsQuery, UserSettingsResponse?>
{
    public async Task<Result<UserSettingsResponse?>> Handle(GetUserSettingsQuery query, CancellationToken cancellationToken)
    {
        var settings = await repository.GetAsync(user.Name!, cancellationToken).ConfigureAwait(false);
        return Result.Success(settings?.ToResponse());
    }
}
