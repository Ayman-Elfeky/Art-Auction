using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(p => p.Name).IsUnique();
    }
}
