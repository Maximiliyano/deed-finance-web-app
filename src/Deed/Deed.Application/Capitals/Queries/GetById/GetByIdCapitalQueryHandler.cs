using Deed.Application.Abstractions.Messaging;
using Deed.Application.Capitals.Responses;
using Deed.Application.Capitals.Specifications;
using Deed.Domain.Errors;
using Deed.Domain.Repositories;
using Deed.Domain.Results;

namespace Deed.Application.Capitals.Queries.GetById;

internal sealed class GetByIdCapitalQueryHandler(ICapitalRepository repository)
    : IQueryHandler<GetByIdCapitalQuery, CapitalResponse>
{
    public async Task<Result<CapitalResponse>> Handle(GetByIdCapitalQuery query, CancellationToken cancellationToken)
    {
        var capital = await repository.GetAsync(new CapitalByIdSpecification(query.Id)).ConfigureAwait(false);

        if (capital is null)
        {
            return Result.Failure<CapitalResponse>(DomainErrors.General.NotFound(nameof(capital)));
        }

        var capitalResponse = capital.ToResponse();

        return Result.Success(capitalResponse);
    }
}
