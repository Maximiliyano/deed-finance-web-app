namespace Deed.Application.Auth.Picture;

public sealed record ProfilePicture(ReadOnlyMemory<byte> Data, string ContentType);
