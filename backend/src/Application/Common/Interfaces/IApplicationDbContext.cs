using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Artwork> Artworks { get; }
    DbSet<Category> Categories { get; }
    DbSet<ArtworkTag> ArtworkTags { get; }
    DbSet<AdminTag> AdminTags { get; }
    DbSet<Bid> Bids { get; }
    DbSet<Watchlist> Watchlists { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRoleAssignment> UserRoleAssignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
