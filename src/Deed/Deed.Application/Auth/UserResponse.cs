using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Auth;

public sealed record UserResponse(
    string? Email,
    bool? EmailVerified,
    string? Fullname,
    Uri? PictureUrl
);
