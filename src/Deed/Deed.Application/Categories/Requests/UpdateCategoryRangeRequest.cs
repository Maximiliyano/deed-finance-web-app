namespace Deed.Application.Categories.Requests;

public sealed record UpdateCategoryRangeRequest(
    IEnumerable<UpdateCategoryRequest> Collection
);
