using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;
using Serilog;

namespace Deed.Application.Capitals.Commands.Create;

internal sealed class CreateCapitalCommandHandler(
    IUser user,
    ICapitalRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCapitalCommand, int>
{
    public async Task<Result<int>> Handle(CreateCapitalCommand command, CancellationToken cancellationToken)
    {
        if (!user.IsAuthenticated)
        {
            var count = await repository.CountAsync(
                new CapitalsByQueryParamsSpecification(user.Name!, disableIncludes: true), cancellationToken).ConfigureAwait(false);
            
            if (count >= AnonymousConstants.EntityLimit)
            {
                return Result.Failure<int>(DomainErrors.Anonymous.LimitReached);
            }
        }

        var capital = command.ToEntity();

        repository.Create(capital);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        Log.Information("Capital - {0} created", capital.Id);

        return Result.Success(capital.Id);
    }
}
