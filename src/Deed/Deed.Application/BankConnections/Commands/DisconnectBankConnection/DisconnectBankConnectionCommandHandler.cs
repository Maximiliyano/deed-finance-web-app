using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.BankConnections.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.BankConnections.Commands.DisconnectBankConnection;

internal sealed class DisconnectBankConnectionCommandHandler(
    IUser user,
    IBankConnectionRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DisconnectBankConnectionCommand>
{
    public async Task<Result> Handle(DisconnectBankConnectionCommand command, CancellationToken cancellationToken)
    {
        var connection = await repository
            .GetAsync(new BankConnectionByIdSpecification(command.Id, user.Name!, tracking: true), cancellationToken)
            .ConfigureAwait(false);

        if (connection is null)
        {
            return Result.Failure(DomainErrors.Bank.NotConnected);
        }

        repository.Delete(connection);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Bank connection {Id} removed for {User}", connection.Id, user.Name);

        return Result.Success();
    }
}
