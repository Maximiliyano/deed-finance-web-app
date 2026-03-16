namespace Deed.Application.Auth;

public interface IUser
{
    string? Email { get; }
    bool? IsEmailVerified { get; }
    string? Name { get; }
    Uri? PictureUrl { get; }
    bool IsAuthenticated { get; }
}
