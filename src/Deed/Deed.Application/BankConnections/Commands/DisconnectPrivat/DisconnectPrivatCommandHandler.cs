using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BankConnections.Commands.DisconnectPrivat;

internal sealed class DisconnectPrivatCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DisconnectPrivatCommand>
{
    public async Task<Result> Handle(DisconnectPrivatCommand command, CancellationToken cancellationToken)
    {
        var connection = await repository
            .GetAsync(new BankConnectionByUserSpecification(user.Name!, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        if (connection is null)
        {
            return Result.Failure(DomainErrors.Bank.NotConnected);
        }

        repository.Delete(connection);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Bank connection removed for {User}", user.Name);

        return Result.Success();
    }
}
