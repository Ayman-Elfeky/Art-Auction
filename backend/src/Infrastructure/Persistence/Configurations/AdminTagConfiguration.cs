using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class AdminTagConfiguration : IEntityTypeConfiguration<AdminTag>
{
    public void Configure(EntityTypeBuilder<AdminTag> builder)
    {
        builder.ToTable("AdminTags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}