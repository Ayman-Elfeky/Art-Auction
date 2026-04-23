using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired();

        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(1024);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.HasMany(u => u.Artworks)
            .WithOne(a => a.Artist)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Bids)
            .WithOne(b => b.Buyer)
            .HasForeignKey(b => b.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Watchlist)
            .WithOne(w => w.Buyer)
            .HasForeignKey(w => w.BuyerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RoleAssignments)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
