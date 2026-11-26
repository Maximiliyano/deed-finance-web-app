using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Requests;

namespace Deed.Application.Categories.Commands.UpdateRange;

public sealed record UpdateCategoriesCommand(
    IEnumerable<UpdateCategoryRequest> Requests
) : ICommand;
