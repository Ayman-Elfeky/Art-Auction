using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Authorization;
using ArtAuction.Domain.Entities;
using ArtAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArtAuction.Infrastructure.Security;

public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;

    public JwtService(IConfiguration configuration, IApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public async Task<string> GenerateToken(User user, CancellationToken cancellationToken = default)
    {
        var key = _configuration["Jwt:Key"] ?? "dev-key-change-me-32-characters-minimum";
        var issuer = _configuration["Jwt:Issuer"] ?? "ArtAuction";
        var audience = _configuration["Jwt:Audience"] ?? "ArtAuctionClients";
        var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var value) ? value : 120;

        var roleWithPermissions = await _context.UserRoleAssignments
            .Where(ua => ua.UserId == user.Id)
            .Select(ua => new
            {
                RoleName = ua.Role.Name,
                Permissions = ua.Role.RolePermissions.Select(rp => rp.Permission.Name)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var roleName = roleWithPermissions?.RoleName ?? user.Role.ToString();
        var permissions = roleWithPermissions?.Permissions.ToArray() ?? GetPermissions(user.Role).ToArray();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(ClaimTypes.Role, roleName)
        }.ToList();

        foreach (var permission in permissions.Distinct())
        {
            claims.Add(new Claim("permission", permission));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static IEnumerable<string> GetPermissions(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => new[]
            {
                Permissions.ManageArtistAccounts,
                Permissions.ReviewArtworks,
                Permissions.CatalogManage,
                Permissions.UsersView,
                Permissions.RolesManage,
                Permissions.PermissionsManage,
                Permissions.RoleAssignmentsManage
            },
            UserRole.Artist => new[]
            {
                Permissions.ArtworksCreate,
                Permissions.ArtworksUpdate,
                Permissions.ArtworksDelete,
                Permissions.ArtworksExtendAuction
            },
            UserRole.Buyer => new[]
            {
                Permissions.BidsPlace,
                Permissions.WatchlistManage
            },
            _ => []
        };
    }
}
