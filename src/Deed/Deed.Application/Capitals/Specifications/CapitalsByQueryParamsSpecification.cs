using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions;
using Deed.Domain.Constants;
using Deed.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Deed.Application.Capitals.Specifications;

internal sealed class CapitalsByQueryParamsSpecification : BaseSpecification<Capital>
{
    public CapitalsByQueryParamsSpecification(string? searchTerm, string? sortBy, string? sortDirection, string? filterBy)
        : base(GetCriteria(filterBy, searchTerm))
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

        AddInclude(c => c.Expenses);
        AddInclude(c => c.Incomes);
        AddInclude(c => c.TransfersIn);
        AddInclude(c => c.TransfersOut);
    }

    private static Expression<Func<Capital, bool>>? GetCriteria(string? filterBy, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            return c => EF.Functions.Like(c.Name, $"%{searchTerm}%");
        }

        return filterBy switch
        {
            FilterKeysConstants.OnlyForSavings => c => !c.OnlyForSavings,
            _ => null
        };
    }

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
