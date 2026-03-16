using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Transfers.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Transfers.Commands.Delete;

internal sealed class DeleteTransferCommandHandler(
    ITransferRepository transferRepository,
    ICapitalRepository capitalRepository,
    IUnitOfWork unitOfWork,
    IUser user)
    : ICommandHandler<DeleteTransferCommand>
{
    public async Task<Result> Handle(DeleteTransferCommand command, CancellationToken cancellationToken)
    {
        var transfer = await transferRepository
            .GetAsync(new TransferByIdSpecification(command.Id, user.Name), cancellationToken)
            .ConfigureAwait(false);

        if (transfer is null)
            return Result.Failure(DomainErrors.General.NotFound("transfer"));

        var source = await capitalRepository
            .GetAsync(new CapitalByIdSpecification(transfer.SourceCapitalId), cancellationToken)
            .ConfigureAwait(false);

        var destination = await capitalRepository
            .GetAsync(new CapitalByIdSpecification(transfer.DestinationCapitalId), cancellationToken)
            .ConfigureAwait(false);

        if (source is not null)
        {
            source.Balance += transfer.Amount;
            capitalRepository.Update(source);
        }

        if (destination is not null)
        {
            destination.Balance -= transfer.DestinationAmount;
            capitalRepository.Update(destination);
        }

        transferRepository.Delete(transfer);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Transfer {Id} deleted and balances reversed", command.Id);

        return Result.Success();
    }
}
