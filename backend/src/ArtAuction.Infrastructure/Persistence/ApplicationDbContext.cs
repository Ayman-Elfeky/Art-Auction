using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Domain.Entities;
using ArtAuction.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Artwork> Artworks { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ArtworkTag> ArtworkTags { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Watchlist> Watchlists { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ArtworkConfiguration());
        modelBuilder.ApplyConfiguration(new ArtworkTagConfiguration());
        modelBuilder.ApplyConfiguration(new BidConfiguration());
        modelBuilder.ApplyConfiguration(new WatchlistConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
    }
}
