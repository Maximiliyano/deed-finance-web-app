using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankNotifications;

internal static class BankConnectionScope
{
    internal static (IReadOnlyCollection<int> Ids, bool IncludeUnassigned) Resolve(
        IEnumerable<BankConnection> connections,
        string? provider,
        int? connectionId)
    {
        BankProvider? parsed = Enum.TryParse<BankProvider>(provider, true, out var value) && value != BankProvider.None
            ? value
            : null;

        var ids = connections
            .Where(c => !c.IsExcluded && (parsed is null || c.Provider == parsed))
            .Select(c => c.Id)
            .ToList();

        return (ids, parsed is null && connectionId is null);
    }
}
