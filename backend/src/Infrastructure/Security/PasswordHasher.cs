using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Security;

namespace ArtAuction.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public string GenerateSalt()
    {
        return PasswordHashing.GenerateSalt();
    }

    public string HashPassword(string password, string salt)
    {
        return PasswordHashing.HashPassword(password, salt);
    }

    public bool VerifyPassword(string password, string salt, string passwordHash)
    {
        return PasswordHashing.VerifyPassword(password, salt, passwordHash);
    }
}