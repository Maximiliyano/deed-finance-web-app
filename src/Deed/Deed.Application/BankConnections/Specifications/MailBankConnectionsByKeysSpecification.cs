using Deed.Application.Abstractions;
using Deed.Domain.Entities;
using Deed.Domain.Enums;

namespace Deed.Application.BankConnections.Specifications;

internal sealed class MailBankConnectionsByKeysSpecification : BaseSpecification<BankConnection>
{
    public MailBankConnectionsByKeysSpecification(IReadOnlyCollection<string> keys)
        : base(c => c.Provider == BankProvider.PrivatMail
            && c.Status == BankConnectionStatus.Active
            && !c.IsExcluded
            && c.ProviderAccountId != null
            && keys.Contains(c.ProviderAccountId))
    {
        Tracking = true;
    }
}
