using Deed.Application.Categories.Commands.Create;
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
}
