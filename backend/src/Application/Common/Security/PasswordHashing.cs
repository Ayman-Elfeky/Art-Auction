using System.Security.Cryptography;

namespace ArtAuction.Application.Common.Security;

public static class PasswordHashing
{
    private const int SaltSize = 32;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string GenerateSalt()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(SaltSize));
    }

    public static string HashPassword(string password, string salt)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        if (string.IsNullOrWhiteSpace(salt))
        {
            throw new ArgumentException("Salt is required.", nameof(salt));
        }

        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(KeySize));
    }

    public static bool VerifyPassword(string password, string salt, string passwordHash)
    {
        if (!string.IsNullOrWhiteSpace(salt))
        {
            var computedHash = HashPassword(password, salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computedHash),
                Convert.FromBase64String(passwordHash));
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}