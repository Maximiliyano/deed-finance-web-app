using Deed.Application.Abstractions;
using Deed.Application.Auth;
using Deed.Domain.Entities;

namespace Deed.Application.Capitals.Specifications;

internal sealed class UserCreatedCapitalsSpecification(string createdBy)
    : BaseSpecification<Capital>(c => c.CreatedBy == createdBy && c.Id > AuthConstants.SeededCapitalMaxId);
