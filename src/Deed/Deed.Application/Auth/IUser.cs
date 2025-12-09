using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Auth;

public interface IUser
{
    string? Email { get; }
    bool? IsEmailVerified { get; }
    string? Name { get; }
    Uri? PictureUrl { get; }
    bool IsAuthenticated { get; }
}
