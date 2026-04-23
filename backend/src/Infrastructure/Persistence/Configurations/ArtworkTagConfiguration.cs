using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public sealed class ArtworkTagConfiguration : IEntityTypeConfiguration<ArtworkTag>
{
    public void Configure(EntityTypeBuilder<ArtworkTag> builder)
    {
        builder.ToTable("ArtworkTags");

        builder.HasKey(at => at.Id);

        builder.Property(at => at.Tag)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(at => new { at.ArtworkId, at.Tag })
            .IsUnique();

        builder.HasOne(at => at.Artwork)
            .WithMany(a => a.Tags)
            .HasForeignKey(at => at.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
