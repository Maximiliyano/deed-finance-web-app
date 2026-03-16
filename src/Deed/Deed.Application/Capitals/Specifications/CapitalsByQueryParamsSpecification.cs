using System.Globalization;
using System.Linq.Expressions;
using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deed.Application.Capitals.Specifications;

internal sealed class CapitalsByQueryParamsSpecification : BaseSpecification<Capital>
{
    public CapitalsByQueryParamsSpecification(string? createdBy, string? searchTerm = null, string? sortBy = null, string? sortDirection = null, string? filterBy = null, bool disableIncludes = false)
        : base(GetCriteria(createdBy, filterBy, searchTerm))
    {
        var keySelector = GetSortProperties(sortBy);

        switch (sortDirection?.ToLower(CultureInfo.CurrentCulture))
        {
            case "asc":
                ApplyOrderBy(keySelector);
                break;
            default:
                ApplyOrderByDescending(keySelector);
                break;
        }

        if (!disableIncludes)
        {
            AddInclude(c => c.Expenses);
            AddInclude(c => c.Incomes);
            AddInclude(c => c.TransfersIn);
            AddInclude(c => c.TransfersOut);
        }
    }

    private static Expression<Func<Capital, bool>>? GetCriteria(string? createdBy, string? filterBy, string? searchTerm)
    {
        Expression<Func<Capital, bool>>? userFilter = !string.IsNullOrEmpty(createdBy)
            ? c => c.CreatedBy == createdBy
            : null;

        Expression<Func<Capital, bool>>? additionalFilter = !string.IsNullOrWhiteSpace(searchTerm)
            ? c => EF.Functions.Like(c.Name, $"%{EscapeLikePattern(searchTerm)}%")
            : filterBy switch
            {
                FilterKeysConstants.OnlyForSavings => c => !c.OnlyForSavings,
                _ => null
            };

#pragma warning disable CA1508 // Avoid dead conditional code — both nullable vars are conditionally assigned
        return userFilter is not null && additionalFilter is not null
            ? ExpressionExtension.Combine(userFilter, additionalFilter)
            : userFilter ?? additionalFilter;
#pragma warning restore CA1508
    }

    private static string EscapeLikePattern(string value)
        => value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

    private static Expression<Func<Capital, object>> GetSortProperties(string? sortBy)
    {
        return sortBy switch
        {
            SortKeysConstants.Name => c => c.Name,
            SortKeysConstants.Balance => c => c.Balance,
            SortKeysConstants.Expenses => c => c.TotalExpense,
            SortKeysConstants.Incomes => c => c.TotalIncome,
            SortKeysConstants.TransfersIn => c => c.TotalTransferIn,
            SortKeysConstants.TransfersOut => c => c.TotalTransferOut,
            _ => c => c.OrderIndex
        };
    }
}
