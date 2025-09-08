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
    public CapitalsByQueryParamsSpecification(string? searchTerm, string? sortBy, string? sortDirection)
        : base(!string.IsNullOrEmpty(searchTerm) ? c => EF.Functions.Like(c.Name, $"%{searchTerm}%") : null)
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
    }

    private static Expression<Func<Capital, object>> GetSortProperties(string? sortBy)
    {
        return sortBy?.ToLower(CultureInfo.CurrentCulture) switch
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
