using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ArtworkTag> ArtworkTags => Set<ArtworkTag>();
    public DbSet<AdminTag> AdminTags => Set<AdminTag>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Watchlist> Watchlists => Set<Watchlist>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
