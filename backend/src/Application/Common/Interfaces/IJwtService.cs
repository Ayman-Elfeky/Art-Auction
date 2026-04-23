using ArtAuction.Domain.Entities;

namespace ArtAuction.Application.Common.Interfaces;

public interface IJwtService
{
    Task<string> GenerateToken(User user, CancellationToken cancellationToken = default);
}
