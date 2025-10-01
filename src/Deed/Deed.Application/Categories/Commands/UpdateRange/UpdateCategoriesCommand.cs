using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deed.Application.Abstractions.Messaging;
using Deed.Application.Categories.Requests;

namespace Deed.Application.Categories.Commands.UpdateRange;

public sealed record UpdateCategoriesCommand(
    IEnumerable<UpdateCategoryRequest> Requests
) : ICommand;
