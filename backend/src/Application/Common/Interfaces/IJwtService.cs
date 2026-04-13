using ArtAuction.Domain.Entities;

namespace ArtAuction.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
