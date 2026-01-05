using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Deed.Application.Auth;

public sealed class User(IHttpContextAccessor accessor)
    : IUser
{
    public string? Email => GetValue(AuthClaimTypes.Email);
    public bool? IsEmailVerified => bool.TryParse(GetValue(AuthClaimTypes.EmailVerified), out var v) && v;
    public string? Name => GetValue(AuthClaimTypes.Name);
    public Uri? PictureUrl
    {
        get
        {
            var p = GetValue(AuthClaimTypes.Picture);
            if (p is not null)
            {
                return new Uri(p);
            }
            return null;
        }
    }
    public bool IsAuthenticated => accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private string? GetValue(string claimType)
    {
        return accessor.HttpContext?.User?.FindFirst(c => c.Type == claimType)?.Value;
    }
}
