using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.PlacedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(b => b.ArtworkId);
        builder.HasIndex(b => b.BuyerId);

        // Relationships
        builder.HasOne(b => b.Artwork)
            .WithMany(a => a.Bids)
            .HasForeignKey(b => b.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Buyer)
            .WithMany(u => u.Bids)
            .HasForeignKey(b => b.BuyerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
