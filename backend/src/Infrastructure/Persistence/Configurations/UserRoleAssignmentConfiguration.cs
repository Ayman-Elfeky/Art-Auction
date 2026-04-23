using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.ToTable("UserRoleAssignments");
        builder.HasKey(ua => new { ua.UserId, ua.RoleId });

        builder.HasIndex(ua => ua.UserId).IsUnique();

        builder.HasOne(ua => ua.User)
            .WithMany(u => u.RoleAssignments)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Role)
            .WithMany(r => r.UserAssignments)
            .HasForeignKey(ua => ua.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
