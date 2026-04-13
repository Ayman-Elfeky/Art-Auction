using ArtAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtAuction.Infrastructure.Persistence.Configurations;

public class ArtworkTagConfiguration : IEntityTypeConfiguration<ArtworkTag>
{
    public void Configure(EntityTypeBuilder<ArtworkTag> builder)
    {
        builder.HasKey(at => at.Id);

        builder.Property(at => at.Tag)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(at => new { at.ArtworkId, at.Tag })
            .IsUnique();

        // Relationships
        builder.HasOne(at => at.Artwork)
            .WithMany(a => a.Tags)
            .HasForeignKey(at => at.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
