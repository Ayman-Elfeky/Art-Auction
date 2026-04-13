using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("Bids");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.PlacedAt)
            .IsRequired();

        builder.Property(b => b.IsWinning)
            .IsRequired();

        builder.HasIndex(b => new { b.ArtworkId, b.PlacedAt });
        builder.HasIndex(b => new { b.BuyerId, b.PlacedAt });

        builder.HasOne(b => b.Artwork)
            .WithMany(a => a.Bids)
            .HasForeignKey(b => b.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Buyer)
            .WithMany(u => u.Bids)
            .HasForeignKey(b => b.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
