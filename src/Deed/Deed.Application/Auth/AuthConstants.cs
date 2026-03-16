using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Auth;

public static class AuthConstants
{
    public const string AuthenticationScheme = "Auth0";

    public const string ResponseType = "code";

    // Marks an intentional login challenge so the OIDC middleware knows
    // to redirect to Auth0 instead of returning 401 for /api requests.
    public const string ExplicitLoginKey = "explicit_login";
}
