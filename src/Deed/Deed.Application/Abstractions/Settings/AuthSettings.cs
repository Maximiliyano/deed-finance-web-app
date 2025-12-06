using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deed.Application.Abstractions.Settings;

internal sealed class AuthSettings
{
    public string Domain { get; init; }
    public string Audience { get; init; }
    public string ClientID { get; init; }
    public string ClientSecret { get; init; }
}
