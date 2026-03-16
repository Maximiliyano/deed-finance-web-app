using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Response;

namespace Deed.Application.Categories.Commands.Restore;

public sealed record RestoreCategoryCommand(int Id) : ICommand<CategoryResponse>;
