using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Application.Transfers.Specifications;
using Deed.Domain.Entities;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Transfers.Commands.Create;

internal sealed class CreateTransferCommandHandler(
    IUser user,
    ICapitalRepository capitalRepository,
    ITransferRepository transferRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTransferCommand, int>
{
    public async Task<Result<int>> Handle(CreateTransferCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await transferRepository.CountAsync(
                new TransfersByUserSpecification(user.Name!), cancellationToken).ConfigureAwait(false);

            if (count >= AuthConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        if (command.SourceCapitalId == command.DestinationCapitalId)
        {
            return Result.Failure<int>(DomainErrors.Exchange.InvalidOperation);
        }

        var source = await capitalRepository
            .GetAsync(new CapitalByIdSpecification(command.SourceCapitalId), cancellationToken)
            .ConfigureAwait(false);

        if (source is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound("source capital"));
        }

        var destination = await capitalRepository
            .GetAsync(new CapitalByIdSpecification(command.DestinationCapitalId), cancellationToken)
            .ConfigureAwait(false);

        if (destination is null)
        {
            return Result.Failure<int>(DomainErrors.General.NotFound("destination capital"));
        }

        source.Balance -= command.Amount;
        destination.Balance += command.DestinationAmount;

        var transfer = new Transfer
        {
            Amount = command.Amount,
            DestinationAmount = command.DestinationAmount,
            SourceCapitalId = command.SourceCapitalId,
            DestinationCapitalId = command.DestinationCapitalId
        };

        capitalRepository.Update(source);
        capitalRepository.Update(destination);
        transferRepository.Create(transfer);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Transfer {Id} created: {Amount} from {Source} to {Dest}",
            transfer.Id, command.Amount, command.SourceCapitalId, command.DestinationCapitalId);

        return Result.Success(transfer.Id);
    }
}
