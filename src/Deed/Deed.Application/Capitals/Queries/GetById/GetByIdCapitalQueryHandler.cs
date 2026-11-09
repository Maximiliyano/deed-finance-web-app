using Deed.Application.Abstractions.Messaging;
using Deed.Application.Auth;
using Deed.Application.Capitals.Responses;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Queries.GetById;

internal sealed class GetByIdCapitalQueryHandler(ICapitalRepository repository, IUser user)
    : IQueryHandler<GetByIdCapitalQuery, CapitalResponse>
{
    public async Task<Result<CapitalResponse>> Handle(GetByIdCapitalQuery query, CancellationToken cancellationToken)
    {
        var summaries = await repository.GetSummariesAsync(new CapitalByIdSpecification(query.Id, user.Name), cancellationToken).ConfigureAwait(false);

        var summary = summaries.SingleOrDefault();

        if (summary is null)
        {
            return Result.Failure<CapitalResponse>(DomainErrors.General.NotFound("capital"));
        }

        return Result.Success(summary.ToResponse());
    }
}
