using Deed.Application.BankConnections.Service;

namespace Deed.Tests.Integration.Infrastructure;

internal sealed class NoopBankBackfillTrigger : IBankBackfillTrigger
{
    public Task TriggerAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
