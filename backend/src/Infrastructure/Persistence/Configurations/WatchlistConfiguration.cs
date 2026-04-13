using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class WatchlistConfiguration : IEntityTypeConfiguration<Watchlist>
{
    public void Configure(EntityTypeBuilder<Watchlist> builder)
    {
        builder.ToTable("Watchlists");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.AddedAt)
            .IsRequired();

        builder.HasIndex(w => new { w.BuyerId, w.ArtworkId })
            .IsUnique();

        builder.HasOne(w => w.Buyer)
            .WithMany(u => u.Watchlist)
            .HasForeignKey(w => w.BuyerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Artwork)
            .WithMany(a => a.Watchlist)
            .HasForeignKey(w => w.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
