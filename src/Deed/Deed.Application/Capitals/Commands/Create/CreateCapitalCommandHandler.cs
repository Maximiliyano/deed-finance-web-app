using Deed.Application.Abstractions.Messaging;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Deed.Application.Capitals.Commands.Create;

internal sealed class CreateCapitalCommandHandler(
    ICapitalRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCapitalCommand, int>
{
    public async Task<Result<int>> Handle(CreateCapitalCommand command, CancellationToken cancellationToken)
    {
        var capital = command.ToEntity();

        repository.Create(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Capital - {0} created", capital.Id);

        return Result.Success(capital.Id);
    }
}
