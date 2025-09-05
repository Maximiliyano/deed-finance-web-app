using Deed.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Deed.Infrastructure.Persistence;

public sealed class ApplicationUser : IdentityUser
{
    public string MainCurrency { get; set; } = CurrencyType.UAH.ToString();
}
