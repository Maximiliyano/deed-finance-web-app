namespace Deed.Application.Auth.Picture;

public interface IProfilePictureService
{
    Task<ProfilePicture?> GetAsync(Uri source, CancellationToken cancellationToken);
}
