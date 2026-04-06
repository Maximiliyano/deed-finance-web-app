namespace Deed.Application.Auth;

public static class AnonymousConstants
{
    public const string SessionCookieName = "deed-anon-id";
    public const string HttpContextItemKey = "deed-anon-id";
    public const int EntityLimit = 3;
}
