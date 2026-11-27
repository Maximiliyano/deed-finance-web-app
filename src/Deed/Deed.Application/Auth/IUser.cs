namespace Deed.Application.Auth;

public interface IUser
{
    string Id { get; }
    
    string? Email { get; }
    
    bool? IsEmailVerified { get; }
    
    string? Name { get; }
    
    IReadOnlyCollection<string> Roles { get; }
    
    Uri? PictureUrl { get; }
    
    bool IsAuthenticated { get; }
}
