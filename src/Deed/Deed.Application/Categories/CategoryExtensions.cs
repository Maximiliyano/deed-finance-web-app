using Deed.Application.Categories.Commands.Create;
using Deed.Application.Categories.Commands.UpdateRange;
using Deed.Application.Categories.Requests;
using Deed.Application.Categories.Response;
using Deed.Domain.Entities;

namespace Deed.Application.Categories;

internal static class CategoryExtensions
{
    internal static CategoryResponse ToResponse(this Category category)
    {
        return new CategoryResponse(
                    category.Id,
                    category.Name,
                    category.Type,
                    category.Period.ToString(),
                    category.PlannedPeriodAmount);
    }

    internal static IEnumerable<CategoryResponse> ToResponses(this IEnumerable<Category> categories)
    {
        return categories.Select(e => e.ToResponse());
    }

    internal static Category ToEntity(this CreateCategoryCommand command)
    {
        return new Category
        {
            Name = command.Name,
            Type = command.Type,
            Period = command.Period,
            PlannedPeriodAmount = command.PlannedPeriodAmount
        };
    }

    internal static Category ToEntity(this UpdateCategoryRequest request)
    {
        return new Category(request.Id)
        {
            Name = request.Name,
            Type = request.Type,
            Period = request.PeriodType,
            PlannedPeriodAmount = request.PeriodAmount
        };
    }
}
