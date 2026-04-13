using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class ArtworkConfiguration : IEntityTypeConfiguration<Artwork>
{
    public void Configure(EntityTypeBuilder<Artwork> builder)
    {
        builder.ToTable("Artworks");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasMaxLength(1000)
            .IsRequired();
            
        builder.Property(a => a.InitialPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.BuyNowPrice)
            .HasPrecision(18, 2);

        builder.Property(a => a.CurrentBid)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.AuctionStartTime)
            .IsRequired();

        builder.Property(a => a.AuctionEndTime)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.ImageUrl)
            .HasMaxLength(1024)
            .IsRequired();
    }
}