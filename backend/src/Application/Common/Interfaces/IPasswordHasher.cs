namespace ArtAuction.Application.Common.Interfaces;

public interface IPasswordHasher
{
	string GenerateSalt();
	string HashPassword(string password, string salt);
	bool VerifyPassword(string password, string salt, string passwordHash);
}
