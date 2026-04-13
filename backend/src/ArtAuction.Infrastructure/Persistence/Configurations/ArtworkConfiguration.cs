using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public class ArtworkConfiguration : IEntityTypeConfiguration<Artwork>
{
    public void Configure(EntityTypeBuilder<Artwork> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .IsRequired();

        builder.Property(a => a.InitialPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.BuyNowPrice)
            .HasPrecision(18, 2);

        builder.Property(a => a.CurrentBid)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.ImageUrl)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(a => a.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(a => a.ArtistId);
        builder.HasIndex(a => a.CategoryId);
        builder.HasIndex(a => a.Status);

        // Relationships
        builder.HasOne(a => a.Artist)
            .WithMany(u => u.Artworks)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Artworks)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Tags)
            .WithOne(at => at.Artwork)
            .HasForeignKey(at => at.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Bids)
            .WithOne(b => b.Artwork)
            .HasForeignKey(b => b.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Watchlist)
            .WithOne(w => w.Artwork)
            .HasForeignKey(w => w.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Notifications)
            .WithOne(n => n.RelatedArtwork)
            .HasForeignKey(n => n.RelatedArtworkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
