namespace Deed.Application.BankConnections.Service;

public interface IBankBackfillTrigger
{
    Task TriggerAsync(CancellationToken cancellationToken);
}
