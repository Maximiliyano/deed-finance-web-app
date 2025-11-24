using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Response;
using ICommand = Deed.Application.Abstractions.Messaging.ICommand;

namespace Deed.Application.Categories.Commands.Restore;

public sealed record RestoreCategoryCommand(
    int Id
) : ICommand<CategoryResponse>;
